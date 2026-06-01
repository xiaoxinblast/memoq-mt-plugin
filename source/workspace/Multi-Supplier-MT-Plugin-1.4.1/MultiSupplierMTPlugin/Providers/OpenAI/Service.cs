using MemoQ.MTInterfaces;
using MultiSupplierMTPlugin.Helpers;
using MultiSupplierMTPlugin.ProvidersCommon.Forms.LLM;
using MultiSupplierMTPlugin.ProvidersCommon.Options.LLM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using LLMSupportLang = MultiSupplierMTPlugin.ProvidersCommon.SupportLanguages.LLM;

namespace MultiSupplierMTPlugin.Providers.OpenAI
{
    class Service : LLMBaseService<GeneralSettings, SecureSettings>
    {
        private static readonly HttpClient _httpClient = new HttpClient();


        public Service(MultiSupplierMTOptions mtOptions, ProviderOptions options)
            : base(mtOptions, options)
        {
        }

        public override string UniqueName { get; set; } = ServiceNames.OpenAI_LLM;

        public override bool IsAvailable { get { return _generalSettings.Checked; } set { } }

        public override bool IsBuiltIn { get; set; } = false;

        public override bool IsLLM { get; set; } = true;

        public override bool IsXmlSupported { get; set; } = true;

        public override bool IsHtmlSupported { get; set; } = true;

        public override bool IsBatchSupported
        {
            get { return _generalSettings.EnableBathTranslate; }

            // 然而，这里并不能保存到配置文件
            set { _generalSettings.EnableBathTranslate = value; }
        }

        public override int MaxSegments
        {
            get { return IsBatchSupported ? _generalSettings.BathTranslateMaxSegments : 1; }

            // 然而，这里并不能保存到配置文件
            set { _generalSettings.BathTranslateMaxSegments = value; }
        }

        public override int MaxCharacters
        {
            get { return IsBatchSupported ? _generalSettings.BathTranslateMaxCharacters : 0; }

            // 然而，这里并不能保存到配置文件
            set { _generalSettings.BathTranslateMaxCharacters = value; }
        }


        public override int MaxQueriesPerWindow { get; set; } = 45;

        public override int WindowSizeMs { get; set; } = 1000;

        public override double Smoothness { get; set; } = 1.0;

        public override int MaxThreadHold { get; set; } = 50;

        public override int FailedTimeoutMs { get; set; } = 0;

        public override int RetryWaitingMs { get; set; } = 0;

        public override int NumberOfRetries { get; set; } = 0;

        public override string ApiKeyLink { get; set; } = "https://platform.openai.com/api-keys";

        public override string ApiDocLink { get; set; } = "https://platform.openai.com/docs/api-reference/chat";

        public override string ModelsLink { get; set; } = "https://platform.openai.com/docs/models";

        public override Dictionary<string, string> SupportLangDic { get; set; } = LLMSupportLang.Dic;

        public override ModelItem[] BuildInModels { get; set; } = new string[]
        {
            "gpt-3.5-turbo",
            "gpt-3.5-turbo-0125",
            "gpt-3.5-turbo-1106",
            "gpt-3.5-turbo-instruct",

            "gpt-4",
            "gpt-4-0613",
            "gpt-4-0314",

            "gpt-4-turbo",
            "gpt-4-turbo-2024-04-09",

            "gpt-4-turbo-preview",
            "gpt-4-0125-preview",
            "gpt-4-1106-vision-preview",

            "gpt-4o",
            "gpt-4o-2024-11-20",
            "gpt-4o-2024-08-06",
            "gpt-4o-2024-05-13",

            "gpt-4o-mini", //推荐
            "gpt-4o-mini-2024-07-18",

            "gpt-4.1",
            "gpt-4.1-2025-04-14",

            "gpt-4.1-mini", //推荐
            "gpt-4.1-mini-2025-04-14",

            "gpt-4.1-nano",
            "gpt-4.1-nano-2025-04-14",

            //"chatgpt-4o-latest"
        }.Select(name => new ModelItem() { UniqueName = name, DisplayName = name }).ToArray();


        public override ProviderOptions ShowConfig()
        {
            using (var form = new OptionsForm(this, _generalSettings, _secureSettings, _mtGeneralSettings, _mtSecureSettings))
            {
                form.ShowDialog();
            }

            return new Options(_generalSettings, _secureSettings);
        }

        public override async Task<List<string>> ListModels(CancellationToken cToken, ProviderOptions tempOptions)
        {
            // 决定哪套配置
            var (g, s) = ResolveOptions(tempOptions);
            var baseUrl = ResolveBaseUrl(g);

            // 发送请求
            var modelResponse = await _httpClient
               .Get(baseUrl + "/models")
               .AddHeader("Authorization", "Bearer " + s.ApiKey)
               .AddHeaderIf(!string.IsNullOrEmpty(s.Organization), "OpenAI-Organization", s.Organization)
               .ReceiveJson<ModelResponse>(cToken);

            // 返回最终结果
            return modelResponse.Data.Select(m => m.Id).OrderBy(i => i, new NaturalSortComparer()).ToList();
        }

        private ThinkingProviderKind ResolveThinkingProviderKind(GeneralSettings g)
        {
            var baseUrl = NormalizeText(g.BaseURL);

            if (ServiceNames.xAI_LLM == UniqueName || baseUrl.Contains("api.x.ai"))
                return ThinkingProviderKind.None;

            if (ServiceNames.Aliyun_LLM == UniqueName || baseUrl.Contains("dashscope.aliyuncs.com"))
                return ThinkingProviderKind.AliyunEnableThinking;

            if (ServiceNames.DeepSeek_LLM == UniqueName || baseUrl.Contains("api.deepseek.com"))
                return ThinkingProviderKind.DeepSeekThinking;

            if (ServiceNames.Google_LLM == UniqueName || baseUrl.Contains("generativelanguage.googleapis.com"))
                return ThinkingProviderKind.GoogleReasoningEffort;

            if (ServiceNames.OpenAI_LLM == UniqueName || baseUrl.Contains("api.openai.com"))
                return ThinkingProviderKind.OpenAIReasoningEffort;

            return ThinkingProviderKind.None;
        }

        private void ApplyThinkingOptions(ChatCompletionRequest chatCompletionRequest, GeneralSettings g)
        {
            switch (ResolveThinkingProviderKind(g))
            {
                case ThinkingProviderKind.AliyunEnableThinking:
                    chatCompletionRequest.EnableThinking = ResolveAliyunEnableThinking(g.ThinkingMode);
                    break;
                case ThinkingProviderKind.DeepSeekThinking:
                    ApplyDeepSeekThinkingOptions(chatCompletionRequest, g);
                    break;
                case ThinkingProviderKind.GoogleReasoningEffort:
                    if (SupportsGoogleReasoningEffort(g.Model))
                    {
                        chatCompletionRequest.ReasoningEffort = ResolveReasoningEffort(
                            g.ThinkingMode,
                            g.ThinkingStrength,
                            ResolveGoogleReasoningEfforts(g.Model));
                    }
                    break;
                case ThinkingProviderKind.OpenAIReasoningEffort:
                    if (SupportsOpenAIReasoningEffort(g.Model))
                    {
                        chatCompletionRequest.ReasoningEffort = ResolveReasoningEffort(
                            g.ThinkingMode,
                            g.ThinkingStrength,
                            ResolveOpenAIReasoningEfforts(g.Model));
                    }
                    break;
            }
        }

        private static void ApplyDeepSeekThinkingOptions(ChatCompletionRequest chatCompletionRequest, GeneralSettings g)
        {
            switch (ResolveDeepSeekModelKind(g.Model))
            {
                case DeepSeekModelKind.NativeThinkingModel:
                    chatCompletionRequest.Thinking = ResolveDeepSeekThinking(g.ThinkingMode);
                    if (g.ThinkingMode != ThinkingMode.Off)
                    {
                        chatCompletionRequest.ReasoningEffort = ResolveDeepSeekReasoningEffort(g.ThinkingStrength);
                    }
                    break;
                case DeepSeekModelKind.LegacyReasonerAlias:
                    chatCompletionRequest.ReasoningEffort = ResolveDeepSeekReasoningEffort(g.ThinkingStrength);
                    break;
            }
        }

        private static bool? ResolveAliyunEnableThinking(ThinkingMode thinkingMode)
        {
            switch (thinkingMode)
            {
                case ThinkingMode.On:
                    return true;
                case ThinkingMode.Off:
                    return false;
                default:
                    return null;
            }
        }

        private static Thinking ResolveDeepSeekThinking(GeneralSettings g)
        {
            return ResolveDeepSeekThinking(g.ThinkingMode);
        }

        private static Thinking ResolveDeepSeekThinking(ThinkingMode thinkingMode)
        {
            if (thinkingMode == ThinkingMode.ProviderDefault)
                return null;

            return new Thinking()
            {
                Type = thinkingMode == ThinkingMode.On ? "enabled" : "disabled"
            };
        }

        private static string ResolveReasoningEffort(
            ThinkingMode thinkingMode,
            ThinkingStrength thinkingStrength,
            HashSet<string> allowedEfforts)
        {
            if (thinkingMode == ThinkingMode.Off)
                return allowedEfforts.Contains("none") ? "none" : null;

            string effort;
            switch (thinkingStrength)
            {
                case ThinkingStrength.None:
                    effort = "none";
                    break;
                case ThinkingStrength.Minimal:
                    effort = "minimal";
                    break;
                case ThinkingStrength.Low:
                    effort = "low";
                    break;
                case ThinkingStrength.Medium:
                    effort = "medium";
                    break;
                case ThinkingStrength.High:
                    effort = "high";
                    break;
                case ThinkingStrength.XHigh:
                    effort = "xhigh";
                    break;
                default:
                    effort = thinkingMode == ThinkingMode.On ? "medium" : null;
                    break;
            }

            return !string.IsNullOrEmpty(effort) && allowedEfforts.Contains(effort) ? effort : null;
        }

        private static string ResolveDeepSeekReasoningEffort(ThinkingStrength thinkingStrength)
        {
            switch (thinkingStrength)
            {
                case ThinkingStrength.Low:
                case ThinkingStrength.Medium:
                case ThinkingStrength.High:
                    return "high";
                case ThinkingStrength.XHigh:
                    return "max";
                default:
                    return null;
            }
        }

        private static bool SupportsOpenAIReasoningEffort(string model)
        {
            var normalized = NormalizeText(model);
            return string.IsNullOrEmpty(normalized) ||
                   normalized.StartsWith("gpt-5") ||
                   normalized.StartsWith("gpt-oss-") ||
                   normalized.StartsWith("o");
        }

        private static HashSet<string> ResolveOpenAIReasoningEfforts(string model)
        {
            var normalized = NormalizeText(model);

            if (normalized.StartsWith("gpt-5") && normalized.Contains("-pro"))
                return new HashSet<string>() { "high" };

            if (normalized.StartsWith("gpt-5.1") ||
                normalized.StartsWith("gpt-5.2") ||
                normalized.StartsWith("gpt-5.3") ||
                normalized.StartsWith("gpt-5.4"))
            {
                var efforts = new HashSet<string>() { "none", "low", "medium", "high" };
                if (normalized.Contains("codex") ||
                    normalized.StartsWith("gpt-5.2") ||
                    normalized.StartsWith("gpt-5.3") ||
                    normalized.StartsWith("gpt-5.4"))
                {
                    efforts.Add("xhigh");
                }
                return efforts;
            }

            if (normalized.StartsWith("gpt-5"))
                return new HashSet<string>() { "minimal", "low", "medium", "high" };

            if (normalized.StartsWith("gpt-oss-") || normalized.StartsWith("o"))
                return new HashSet<string>() { "low", "medium", "high" };

            return string.IsNullOrEmpty(normalized)
                ? new HashSet<string>() { "none", "minimal", "low", "medium", "high", "xhigh" }
                : new HashSet<string>();
        }

        private static bool SupportsGoogleReasoningEffort(string model)
        {
            var normalized = NormalizeText(model);
            return string.IsNullOrEmpty(normalized) ||
                   normalized.StartsWith("gemini-2.5") ||
                   normalized.StartsWith("gemini-3");
        }

        private static HashSet<string> ResolveGoogleReasoningEfforts(string model)
        {
            var normalized = NormalizeText(model);
            var efforts = new HashSet<string>() { "low", "medium", "high" };
            if (string.IsNullOrEmpty(normalized) || normalized.StartsWith("gemini-2.5-flash"))
                efforts.Add("none");
            return efforts;
        }

        private static DeepSeekModelKind ResolveDeepSeekModelKind(string model)
        {
            var normalized = NormalizeText(model);

            if (string.IsNullOrEmpty(normalized))
                return DeepSeekModelKind.NativeThinkingModel;

            if (normalized.StartsWith("deepseek-v"))
                return DeepSeekModelKind.NativeThinkingModel;

            if ("deepseek-reasoner" == normalized)
                return DeepSeekModelKind.LegacyReasonerAlias;

            if ("deepseek-chat" == normalized)
                return DeepSeekModelKind.LegacyChatAlias;

            return DeepSeekModelKind.None;
        }

        private string ResolveBaseUrl(GeneralSettings g)
        {
            var trimmedBaseUrl = (g.BaseURL ?? string.Empty).TrimEnd('/');
            var normalized = NormalizeText(trimmedBaseUrl);

            if ((ServiceNames.DeepSeek_LLM == UniqueName || normalized.Contains("api.deepseek.com")) &&
                "https://api.deepseek.com/v1" == normalized)
            {
                return "https://api.deepseek.com";
            }

            return trimmedBaseUrl;
        }

        private static string NormalizeText(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim().ToLowerInvariant();
        }

        protected override async Task<string> TranslateAsync(GeneralSettings g, SecureSettings s, string systemPrompt, string userPrompt, CancellationToken cToken)
        {
            var localizedName = ServiceLocalizedNameHelper.Get(UniqueName);
            var baseUrl = ResolveBaseUrl(g);

            // 6.请求体
            var chatCompletionRequest = new ChatCompletionRequest()
            {
                Messages = new Message[]
                {
                    new Message(){ Role = "system", Content = systemPrompt},
                    new Message(){ Role = "user", Content = userPrompt}
                },

                Model = g.Model,

                MaxTokens = g.MaxTokens,
                Temperature = g.Temperature,
            };
            ApplyThinkingOptions(chatCompletionRequest, g);

            // 7.请求体批量翻译格式处理
            if (g.EnableBathTranslate)
            {
                var responseFormat = new ResponseFormat();
                switch (g.BathTranslateResponseFormat)
                {
                    case BathTranslateResponseFormat.JSON_Schema:
                        responseFormat.Type = "json_schema";
                        responseFormat.JsonSchema = BathTranslateHelper.GetJsonScheme(g.BathTranslateSchema);
                        break;
                    case BathTranslateResponseFormat.JSON_Object:
                        responseFormat.Type = "json_object";
                        break;
                    default:
                        responseFormat.Type = "text";
                        break;
                }
                chatCompletionRequest.ResponseFormat = responseFormat;
            }

            // 8.发送请求
            var chatCompletionResponse = await _httpClient
                .Post(baseUrl + g.Path)
                .AddHeader("Authorization", "Bearer " + s.ApiKey)
                .AddHeaderIf(!string.IsNullOrEmpty(s.Organization), "OpenAI-Organization", s.Organization)
                .SetBodyJson(chatCompletionRequest)
                .ReceiveJson<ChatCompletionResponse>(cToken);

            // 9.读取结果中关心的字段
            var choice = chatCompletionResponse.Choices[0];

            // 10.非正常响应不能当作成功结果，否则 length/refusal 会缓存空译文
            if (!"stop".Equals(choice?.FinishReason, StringComparison.OrdinalIgnoreCase) || !string.IsNullOrEmpty(choice?.Message?.Refusal))
            {
                var abnormalFinishMessage = $"{localizedName} Abnormal Finish Reason\r\n" +
                    $"Response did not complete normally — expected finish reason 'stop', but got '{choice?.FinishReason}'.\r\n" +
                    $"{choice?.Message?.Refusal}";
                LoggingHelper.Warn(abnormalFinishMessage);
                throw new Exception(abnormalFinishMessage);
            }

            // 11.读取翻译结果内容字段
            string content = choice.Message.Content;

            // 12.日志记录响应结果
            LoggingHelper.Info("-- Response --");
            LoggingHelper.Multiline(content);

            // 13.日志记录 token 使用情况
            LoggingHelper.Info($"Tokens | In={chatCompletionResponse?.Usage?.PromptTokens}" +
                $"(cacheW={chatCompletionResponse?.Usage?.PromptTokens},cacheR={chatCompletionResponse?.Usage?.PromptTokensDetails?.CachedTokens})" +
                $" · Out={chatCompletionResponse?.Usage?.CompletionTokens}" +
                $" · Reasoning={chatCompletionResponse?.Usage?.CompletionTokensDetails?.ReasoningTokens}");

            return content;
        }

        enum ThinkingProviderKind
        {
            None,
            OpenAIReasoningEffort,
            GoogleReasoningEffort,
            DeepSeekThinking,
            AliyunEnableThinking
        }

        enum DeepSeekModelKind
        {
            None,
            NativeThinkingModel,
            LegacyChatAlias,
            LegacyReasonerAlias
        }
    }
}
