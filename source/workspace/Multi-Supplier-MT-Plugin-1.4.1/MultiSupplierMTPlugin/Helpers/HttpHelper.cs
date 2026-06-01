using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace MultiSupplierMTPlugin.Helpers
{
    static class FluentHttpClientExtensions
    {
        public static HttpRequestBuilder Get(this HttpClient client, string url) => new HttpRequestBuilder(client, HttpMethod.Get, url);
        public static HttpRequestBuilder Post(this HttpClient client, string url) => new HttpRequestBuilder(client, HttpMethod.Post, url);
    }

    class HttpRequestBuilder
    {
        private static long _requestSequence;

        private readonly HttpClient _client;
        private readonly UriBuilder _uriBuilder;
        private readonly HttpRequestMessage _request;
        private readonly List<KeyValuePair<string, string>> _queryParams;

        public HttpRequestBuilder(HttpClient client, HttpMethod method, string url)
        {
            _client = client;
            _uriBuilder = new UriBuilder(url);
            _request = new HttpRequestMessage(method, _uriBuilder.Uri);
            _queryParams = new List<KeyValuePair<string, string>>();

            var parsed = HttpUtility.ParseQueryString(_uriBuilder.Query ?? "");
            foreach (string key in parsed.Keys)
            {
                if (key != null)
                    _queryParams.Add(new KeyValuePair<string, string>(key, parsed[key]));
            }
        }

        public HttpRequestBuilder AddHeader(string key, string value)
        {
            _request.Headers.TryAddWithoutValidation(key, value);
            return this;
        }

        public HttpRequestBuilder AddHeaders(IEnumerable<KeyValuePair<string, string>> headers)
        {
            foreach (var header in headers)
                AddHeader(header.Key, header.Value);
            return this;
        }

        public HttpRequestBuilder AddHeaderIf(bool condition, string key, string value)
        {
            return condition ? AddHeader(key, value) : this;
        }

        public HttpRequestBuilder AddHeadersIf(bool condition, IEnumerable<KeyValuePair<string, string>> headers)
        {
            return condition ? AddHeaders(headers) : this;
        }

        public HttpRequestBuilder AddQuery(string key, string value)
        {
            _queryParams.Add(new KeyValuePair<string, string>(key, value));
            return this;
        }

        public HttpRequestBuilder AddQueries(IEnumerable<KeyValuePair<string, string>> queries)
        {
            foreach (var query in queries)
                AddQuery(query.Key, query.Value);
            return this;
        }

        public HttpRequestBuilder AddQueryIf(bool condition, string key, string value)
        {
            return condition ? AddQuery(key, value) : this;
        }

        public HttpRequestBuilder AddQueriesIf(bool condition, IEnumerable<KeyValuePair<string, string>> queries)
        {
            return condition ? AddQueries(queries) : this;
        }

        public HttpRequestBuilder SetBearerToken(string token)
        {
            _request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return this;
        }

        public HttpRequestBuilder SetBearerTokenIf(bool condition, string token)
        {
            return condition ? SetBearerToken(token) : this;
        }

        public HttpRequestBuilder SetBodyForm(Dictionary<string, string> formFields)
        {
            return SetBodyForm(formFields?.Select(kvp => new KeyValuePair<string, string>(kvp.Key, kvp.Value)));
        }

        public HttpRequestBuilder SetBodyForm(IEnumerable<KeyValuePair<string, string>> formFields)
        {
            _request.Content = new FormUrlEncodedContent(formFields ?? Enumerable.Empty<KeyValuePair<string, string>>());
            return this;
        }

        public HttpRequestBuilder SetBodyJson(object body)
        {
            var json = JsonConvert.SerializeObject(body);
            _request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            return this;
        }

        public HttpRequestBuilder SetBodyJsonString(string jsonString)
        {
            _request.Content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            return this;
        }

        public HttpRequestBuilder SetBodyJsonByteArray(byte[] jsonByteArray)
        {
            var content = new ByteArrayContent(jsonByteArray);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            _request.Content = content;
            return this;
        }

        public async Task<string> ReceiveString(CancellationToken cancellationToken = default)
        {
            return await SendAsync(cancellationToken);
        }

        public async Task<T> ReceiveJson<T>(CancellationToken cancellationToken = default)
        {
            var content = await SendAsync(cancellationToken);

            try
            {
                return JsonConvert.DeserializeObject<T>(content);
            }
            catch (Exception ex)
            {
                throw new Exception($"JSON deserialization failed: {ex.Message}. Content\r\n: {content}");
            }
        }

        private async Task<string> SendAsync(CancellationToken cancellationToken = default)
        {
            if (_queryParams.Count > 0)
            {
                var queryString = string.Join("&", _queryParams.Select(kv =>
                    $"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}"));
                _uriBuilder.Query = queryString;
            }
            _request.RequestUri = _uriBuilder.Uri;

            var requestId = Interlocked.Increment(ref _requestSequence);
            LoggingHelper.Api(await BuildRequestLogAsync(requestId, _request));

            var stopwatch = Stopwatch.StartNew();
            HttpResponseMessage response = null;

            try
            {
                response = await _client.SendAsync(_request, cancellationToken);

                var content = response.Content == null ? string.Empty : await response.Content.ReadAsStringAsync();
                stopwatch.Stop();

                LoggingHelper.Api(BuildResponseLog(requestId, response, content, stopwatch.ElapsedMilliseconds));

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Http Request Exception {(int)response.StatusCode} {response.ReasonPhrase}.\r\n{content}");
                }

                return content;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                LoggingHelper.Api(BuildExceptionLog(requestId, ex, stopwatch.ElapsedMilliseconds));
                throw;
            }
            finally
            {
                response?.Dispose();
                _request.Dispose();
            }
        }

        private static async Task<string> BuildRequestLogAsync(long requestId, HttpRequestMessage request)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"HTTP Request #{requestId}");
            builder.AppendLine($"Method: {request.Method}");
            builder.AppendLine($"Uri: {FormatUri(request.RequestUri)}");
            builder.AppendLine("Headers:");
            builder.AppendLine(IndentLines(FormatHeaders(request.Headers)));
            builder.AppendLine("Content Headers:");
            builder.AppendLine(IndentLines(FormatHeaders(request.Content?.Headers)));
            builder.AppendLine("Body:");
            builder.Append(IndentLines(await ReadContentAsStringSafeAsync(request.Content)));
            return builder.ToString();
        }

        private static string BuildResponseLog(long requestId, HttpResponseMessage response, string content, long elapsedMs)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"HTTP Response #{requestId}");
            builder.AppendLine($"Status: {(int)response.StatusCode} {response.ReasonPhrase}");
            builder.AppendLine($"ElapsedMs: {elapsedMs}");
            builder.AppendLine("Headers:");
            builder.AppendLine(IndentLines(FormatHeaders(response.Headers)));
            builder.AppendLine("Content Headers:");
            builder.AppendLine(IndentLines(FormatHeaders(response.Content?.Headers)));
            builder.AppendLine("Body:");
            builder.Append(IndentLines(FormatContent(content)));
            return builder.ToString();
        }

        private static string BuildExceptionLog(long requestId, Exception ex, long elapsedMs)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"HTTP Request #{requestId} Exception");
            builder.AppendLine($"ElapsedMs: {elapsedMs}");
            builder.Append(ex.ToString());
            return builder.ToString();
        }

        private static async Task<string> ReadContentAsStringSafeAsync(HttpContent content)
        {
            if (content == null)
                return "<empty>";

            try
            {
                return FormatContent(await content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                return $"<failed to read body: {ex.Message}>";
            }
        }

        private static string FormatUri(Uri uri)
        {
            if (uri == null)
                return string.Empty;

            var safeBuilder = new UriBuilder(uri);
            var parsed = HttpUtility.ParseQueryString(safeBuilder.Query ?? string.Empty);
            var safeParams = new List<string>();

            foreach (string key in parsed.Keys)
            {
                if (key == null)
                    continue;

                var value = parsed[key];
                safeParams.Add($"{WebUtility.UrlEncode(key)}={WebUtility.UrlEncode(RedactIfNeeded(key, value))}");
            }

            safeBuilder.Query = string.Join("&", safeParams);
            return safeBuilder.Uri.ToString();
        }

        private static string FormatHeaders(HttpHeaders headers)
        {
            if (headers == null || !headers.Any())
                return "<none>";

            return string.Join("\r\n", headers.Select(header =>
            {
                var values = header.Value ?? Enumerable.Empty<string>();
                return $"{header.Key}: {string.Join(", ", values.Select(value => RedactIfNeeded(header.Key, value)))}";
            }));
        }

        private static string IndentLines(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "  <empty>";

            return "  " + text.Replace("\r\n", "\r\n  ").Replace("\n", "\n  ");
        }

        private static string FormatContent(string content)
        {
            return string.IsNullOrEmpty(content) ? "<empty>" : content;
        }

        private static string RedactIfNeeded(string key, string value)
        {
            if (!ShouldRedact(key))
                return value ?? string.Empty;

            if (string.Equals(key, "Authorization", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(value))
            {
                var firstSpaceIndex = value.IndexOf(' ');
                if (firstSpaceIndex > 0)
                    return value.Substring(0, firstSpaceIndex) + " <redacted>";
            }

            return "<redacted>";
        }

        private static bool ShouldRedact(string key)
        {
            var normalized = (key ?? string.Empty).Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(normalized))
                return false;

            return normalized == "authorization" ||
                   normalized == "proxy-authorization" ||
                   normalized == "x-api-key" ||
                   normalized == "api-key" ||
                   normalized == "apikey" ||
                   normalized == "password" ||
                   normalized.EndsWith("-api-key") ||
                   normalized.EndsWith("_api_key") ||
                   normalized.EndsWith("access_key") ||
                   normalized.Contains("token") ||
                   normalized.Contains("secret") ||
                   normalized.Contains("signature") ||
                   normalized.Contains("credential");
        }
    }
}
