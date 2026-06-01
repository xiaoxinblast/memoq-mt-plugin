using MemoQ.PreviewInterfaces;
using MemoQ.PreviewInterfaces.Entities;
using MemoQ.PreviewInterfaces.Exceptions;
using MemoQ.PreviewInterfaces.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MultiSupplierMTPlugin.Helpers
{
    class ContextHelper : IPreviewToolCallback
    {
        private static readonly IReadOnlyDictionary<string, string[]> _previewPropertyAliases =
            new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                [PropertyNames.LineLengthLimit] = new[] { "LineLengthLimit", "Line Length Limit", "LengthLimit", "FullWidthLimit", "Full Width Limit", "MaxLineLength" },
                [PropertyNames.CharCount] = new[] { "CharCount", "CharacterCount", "Characters", "Length" },
            };

        // 单例
        private static volatile ContextHelper _instance = null;
        private static readonly object _singleLockObj = new object();

        // 记录文档的内容
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<int, Content>> _docDic = new ConcurrentDictionary<string, ConcurrentDictionary<int, Content>>();

        // 记录文档的最大索引
        private readonly ConcurrentDictionary<string, int> _lastIndexDic = new ConcurrentDictionary<string, int>(); // TODO：应该和 docDic 合并，否则两者不是同步的

        // 记录文档当前激活的索引
        private readonly ConcurrentDictionary<string, CurrentIndex> _currentIndexDic = new ConcurrentDictionary<string, CurrentIndex>();

        // 记录文档的原始名字
        private readonly ConcurrentDictionary<string, string> _docNameDic = new ConcurrentDictionary<string, string>();

        // 记录语言对到文档键的映射，用于 View 场景下 metaData.DocumentID（View GUID）与
        // Preview SDK 的 SourceDocument.DocumentGuid（原始文档 GUID）不一致时的回退查找
        private readonly ConcurrentDictionary<string, string> _langPairToDocKey = new ConcurrentDictionary<string, string>();

        // 记录句段索引与 preview part id 的映射，用于按需主动刷新当前句段内容
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<int, string>> _previewPartIdDic = new ConcurrentDictionary<string, ConcurrentDictionary<int, string>>();

        // 发送请求
        private readonly MemoqRequest _request = null;


        private ContextHelper(string dllFileName)
        {
            // 初始化请求对象
            _request = new MemoqRequest(this, dllFileName);

            Task.Run(() =>
            {
                try
                {
                    Thread.Sleep(new Random().Next(3000, 10000));

                    _request.ConnetOrRegister();
                }
                catch
                {
                    // Do Nothing.
                }
            });
        }

        public static ContextHelper Instance
        {
            get
            {
                return _instance;
            }
        }

        public static void Init(string dllFileName)
        {
            if (_instance == null)
            {
                lock (_singleLockObj)
                {
                    if (_instance == null)
                    {
                        _instance = new ContextHelper(dllFileName);
                    }
                }
            }
        }


        public CurrentIndex GetCurrentIndex(string prjGuid, string docGuid, string srcLang, string tgtLang)
        {
            string key = ResolveKey(docGuid, srcLang, tgtLang);

            if (!_currentIndexDic.TryGetValue(key, out var currentIndex))
                throw new Exception("Wait for the document to reload and reactivate the current segment, or document load fails, reopen the document.");

            return currentIndex;
        }

        public void ResetCurrentIndex(string prjGuid, string docGuid, string srcLang, string tgtLang)
        {
            string key = ResolveKey(docGuid, srcLang, tgtLang);

            if (!_currentIndexDic.ContainsKey(key))
                throw new Exception("document load fails, reopen the document.");

            _currentIndexDic[key] = new CurrentIndex()
            {
                IndexStart = -1,
                IndexEnd = -1,
                UtcMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };
        }

        public string GetTargetText(string prjGuid, string docGuid, string srcLang, string tgtLang,
            int segmIndex)
        {
            CheckConnect();

            string key = ResolveKey(docGuid, srcLang, tgtLang);

            if (!_docDic.TryGetValue(key, out var doc) || !doc.TryGetValue(segmIndex, out Content content))
                throw new Exception("document load fails, reopen the document.");

            return content.Target;
        }

        public string GetPreviewProperty(string prjGuid, string docGuid, string srcLang, string tgtLang,
            int segmIndex, string propertyName)
        {
            CheckConnect();

            string key = ResolveKey(docGuid, srcLang, tgtLang);

            if (!_docDic.TryGetValue(key, out var doc) || !doc.TryGetValue(segmIndex, out Content content))
                throw new Exception("document load fails, reopen the document.");

            if (TryGetPreviewPropertyValue(content.Properties, propertyName, out var value))
                return value;

            if (TryRefreshPreviewProperty(key, segmIndex, tgtLang, propertyName, out var refreshedValue))
                return refreshedValue;

            if (string.Equals(propertyName, PropertyNames.CharCount, StringComparison.OrdinalIgnoreCase))
            {
                var fallbackText = !string.IsNullOrEmpty(content.Target) ? content.Target : content.Source;
                if (!string.IsNullOrEmpty(fallbackText))
                {
                    var computedValue = fallbackText.Length.ToString();
                    LoggingHelper.Verbose($"Preview property fallback used. Key={key}, SegmentIndex={segmIndex}, Property={propertyName}, Value={computedValue}");
                    return computedValue;
                }
            }

            LoggingHelper.Verbose($"Preview property unavailable. Key={key}, SegmentIndex={segmIndex}, Property={propertyName}");
            return string.Empty;
        }

        public string GetAboveContext(string prjGuid, string docGuid, string srcLang, string tgtLang,
            int segmIndex, int maxSegm, int maxChar, bool includeSrc, bool includeTgt, Func<string, string, string> targetNormalizer = null)
        {
            CheckConnect();

            return GetContext(prjGuid, docGuid, srcLang, tgtLang,
                segmIndex, maxSegm, maxChar, includeSrc, includeTgt, true, targetNormalizer);
        }

        public string GetBelowContext(string prjGuid, string docGuid, string srcLang, string tgtLang,
            int segmIndex, int maxSegm, int maxChar, bool includeSrc, bool includeTgt, Func<string, string, string> targetNormalizer = null)
        {
            CheckConnect();

            return GetContext(prjGuid, docGuid, srcLang, tgtLang,
                segmIndex, maxSegm, maxChar, includeSrc, includeTgt, false, targetNormalizer);
        }

        public string GetDocName(string prjGuid, string docGuid, string srcLang, string tgtLang)
        {
            CheckConnect();

            string key = ResolveKey(docGuid, srcLang, tgtLang);

            if (!_docNameDic.TryGetValue(key, out var name))
                throw new Exception("document load fails, reopen the document.");

            return name;
        }

        public string GetFullText(string prjGuid, string docGuid, string srcLang, string tgtLang)
        {
            CheckConnect();

            string key = ResolveKey(docGuid, srcLang, tgtLang);

            if (!_docDic.TryGetValue(key, out var doc) || !_lastIndexDic.TryGetValue(key, out var lastIndex))
                throw new Exception("document load fails, reopen the document.");

            StringBuilder result = new StringBuilder();

            for (int i = 1; i <= lastIndex; i++)
            {
                if (!doc.TryGetValue(i, out Content content))
                    throw new Exception("document load fails, reopen the document.");

                result.Append(content.Source);
            }

            return result.ToString();
        }

        public void SetContext(string prjGuid, string docGuid, string srcLang, string tgtLang,
           int segmIndex, string srcContent, string tgtContent)
        {
            CheckConnect();

            string key = ResolveKey(docGuid, srcLang, tgtLang);

            SetContext(key, segmIndex, srcContent, tgtContent);
        }


        private void SetDocName(PreviewPart previewPart)
        {
            string key = GetKey(previewPart);

            _docNameDic[key] = previewPart.SourceDocument.DocumentName;
        }

        private string GetContext(string prjGuid, string docGuid, string srcLang, string tgtLang,
           int segmIndex, int maxSegm, int maxChar, bool includeSrc, bool includeTgt, bool isAbove, Func<string, string, string> targetNormalizer)
        {
            string key = ResolveKey(docGuid, srcLang, tgtLang);

            if (!_docDic.TryGetValue(key, out var doc) || !_lastIndexDic.TryGetValue(key, out var lastIndex))
                throw new Exception("document load fails, reopen the document.");

            if ((!includeSrc && !includeTgt) || (maxSegm <= 0 && maxChar <= 0)) return "";

            int direction = isAbove ? -1 : 1;
            int current = segmIndex + direction;

            int charCount = 0;
            int segmCount = 0;
            List<string> results = new List<string>();
            while ((maxSegm <= 0 || segmCount < maxSegm) && current >= 1 && current <= lastIndex)
            {
                if (!doc.TryGetValue(current, out Content content))
                    throw new Exception("document load fails, reopen the document.");

                // 跳过空白句段
                bool srcWhiteSpace = string.IsNullOrWhiteSpace(content.Source);
                bool tgtWhiteSpace = string.IsNullOrWhiteSpace(content.Target);
                bool noSkip = (includeSrc && !srcWhiteSpace) || (includeTgt && !tgtWhiteSpace);
                if (!noSkip)
                {
                    current += direction;
                    continue;
                }

                // 字符计算，不包括额外添加的换行符。
                charCount += includeSrc ? content.Source.Length : 0;
                charCount += includeTgt ? content.Target.Length : 0;
                if (maxChar > 0 && charCount > maxChar) break;

                var normalizedTarget = includeTgt && targetNormalizer != null
                    ? targetNormalizer(content.Source, content.Target)
                    : content.Target;

                if (isAbove)
                {
                    if (includeTgt) results.Insert(0, normalizedTarget);
                    if (includeSrc) results.Insert(0, content.Source);
                }
                else
                {
                    if (includeSrc) results.Add(content.Source);
                    if (includeTgt) results.Add(normalizedTarget);
                }
                segmCount++;

                current += direction;
            }

            return string.Join(Environment.NewLine, results);
        }


        private void SetContext(PreviewPart previewPart)
        {
            string key = GetKey(previewPart);

            int segmIndex = int.Parse(previewPart.PreviewPartId.Split('-').Last());
            string srcContent = previewPart.SourceContent.Content;
            string tgtContent = previewPart.TargetContent.Content;
            var properties = ConvertPreviewProperties(previewPart.PreviewProperties);

            SetPreviewPartId(key, segmIndex, previewPart.PreviewPartId);
            SetContext(key, segmIndex, srcContent, tgtContent, properties);

            if (properties.Count > 0)
                LoggingHelper.Verbose($"Preview properties updated. Key={key}, SegmentIndex={segmIndex}, Properties={FormatPreviewProperties(properties)}");
        }

        private void SetContext(string key, int segmIndex, string srcContent, string tgtContent)
        {
            SetContext(key, segmIndex, srcContent, tgtContent, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
        }

        private void SetContext(string key, int segmIndex, string srcContent, string tgtContent, Dictionary<string, string> properties)
        {
            if (!_docDic.TryGetValue(key, out var doc))
            {
                doc = new ConcurrentDictionary<int, Content>();
                _docDic[key] = doc;

                // 注册语言对 → 文档键映射，用于 View 场景下 GUID 不匹配时的回退
                var parts = key.Split('|');
                if (parts.Length >= 3)
                {
                    var langKey = $"*|{parts[1]}|{parts[2]}";
                    _langPairToDocKey[langKey] = key;
                }
            }
            doc[segmIndex] = new Content(srcContent, tgtContent, properties);

            if (!_lastIndexDic.TryGetValue(key, out var lastIndex))
            {
                lastIndex = 0;
            }
            if (segmIndex > lastIndex)
            {
                _lastIndexDic[key] = segmIndex;
            }
        }

        private Dictionary<string, string> ConvertPreviewProperties(PreviewProperty[] previewProperties)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (previewProperties == null)
                return result;

            foreach (var property in previewProperties)
            {
                if (property == null || string.IsNullOrWhiteSpace(property.Name))
                    continue;

                result[property.Name] = property.Value?.ToString() ?? string.Empty;
            }

            return result;
        }

        private static bool TryGetPreviewPropertyValue(IReadOnlyDictionary<string, string> properties, string propertyName, out string value)
        {
            value = string.Empty;

            if (properties == null || properties.Count == 0 || string.IsNullOrWhiteSpace(propertyName))
                return false;

            if (properties.TryGetValue(propertyName, out var directValue) && !string.IsNullOrWhiteSpace(directValue))
            {
                value = directValue;
                return true;
            }

            if (!_previewPropertyAliases.TryGetValue(propertyName, out var aliases))
                aliases = new[] { propertyName };

            foreach (var alias in aliases)
            {
                if (properties.TryGetValue(alias, out var aliasValue) && !string.IsNullOrWhiteSpace(aliasValue))
                {
                    value = aliasValue;
                    return true;
                }
            }

            foreach (var pair in properties)
            {
                if (string.IsNullOrWhiteSpace(pair.Value))
                    continue;

                if (aliases.Any(alias => pair.Key.IndexOf(alias, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    value = pair.Value;
                    return true;
                }
            }

            return false;
        }

        private static string FormatPreviewProperties(IReadOnlyDictionary<string, string> properties)
        {
            if (properties == null || properties.Count == 0)
                return "<none>";

            return string.Join("; ", properties
                .OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase)
                .Select(p => $"{p.Key}={p.Value}"));
        }

        private void SetPreviewPartId(string key, int segmIndex, string previewPartId)
        {
            if (string.IsNullOrWhiteSpace(previewPartId))
                return;

            if (!_previewPartIdDic.TryGetValue(key, out var doc))
            {
                doc = new ConcurrentDictionary<int, string>();
                _previewPartIdDic[key] = doc;
            }

            doc[segmIndex] = previewPartId;
        }

        private bool TryRefreshPreviewProperty(string key, int segmIndex, string tgtLang, string propertyName, out string value)
        {
            value = string.Empty;

            if (!_previewPartIdDic.TryGetValue(key, out var previewPartIds) ||
                !previewPartIds.TryGetValue(segmIndex, out var previewPartId) ||
                string.IsNullOrWhiteSpace(previewPartId))
            {
                return false;
            }

            try
            {
                _request.RequestRuntimeSettingsChange(new ChangeRuntimeSettingsRequest(
                    ContentComplexityLevel.Minimal,
                    PropertyNames.SupportedProperties));

                LoggingHelper.Verbose($"Preview property refresh requested. Key={key}, SegmentIndex={segmIndex}, Property={propertyName}, PreviewPartId={previewPartId}");
                _request.RequestContentUpdate(new ContentUpdateRequestFromPreviewTool(new[] { previewPartId }, new[] { tgtLang }));

                var startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                while (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - startTime <= 1500)
                {
                    if (_docDic.TryGetValue(key, out var doc) &&
                        doc.TryGetValue(segmIndex, out Content content) &&
                        TryGetPreviewPropertyValue(content.Properties, propertyName, out var refreshedValue))
                    {
                        value = refreshedValue;
                        LoggingHelper.Verbose($"Preview property refresh succeeded. Key={key}, SegmentIndex={segmIndex}, Property={propertyName}, Value={value}");
                        return true;
                    }

                    Thread.Sleep(50);
                }
            }
            catch (Exception ex)
            {
                LoggingHelper.Verbose($"Preview property refresh failed. Key={key}, SegmentIndex={segmIndex}, Property={propertyName}, {ex.GetType().Name}: {ex.Message}");
            }

            return false;
        }


        private string GetKey(PreviewPart previewPart)
        {
            string prjGuid = string.Empty;
            string docGuid = previewPart.SourceDocument.DocumentGuid.ToString();
            string srcLang = previewPart.SourceLangCode;
            string tgtLang = previewPart.TargetLangCode;

            return GetKey(prjGuid, docGuid, srcLang, tgtLang);
        }

        private static string GetKey(string projectGuid, string docGuid, string srcLang, string tgtLang)
        {
            // 暂时不使用 project guid，因为只有 MT SDK 中能获取到，Preview SDK 中获取不到。
            return $"{docGuid}|{srcLang}|{tgtLang}";
        }

        /// <summary>
        /// 解析文档查找键。当 View 场景下 metaData.DocumentID（View GUID）与
        /// Preview SDK 回调中的 SourceDocument.DocumentGuid（原始文档 GUID）不一致时，
        /// 通过语言对反向查找回退到正确的键。
        /// </summary>
        private string ResolveKey(string docGuid, string srcLang, string tgtLang)
        {
            string key = GetKey(null, docGuid, srcLang, tgtLang);

            // 1. 精确匹配（正常文档）
            if (_docDic.ContainsKey(key) || _currentIndexDic.ContainsKey(key))
                return key;

            // 2. View 回退：通过语言对查找（大多数用户只打开一个文档/View）
            var langKey = $"*|{srcLang}|{tgtLang}";
            if (_langPairToDocKey.TryGetValue(langKey, out var resolvedKey))
            {
                LoggingHelper.Verbose($"View document ID resolved. RequestedKey={key}, ResolvedKey={resolvedKey}");
                return resolvedKey;
            }

            // 3. 遍历查找匹配语言对的唯一文档
            var langSuffix = $"|{srcLang}|{tgtLang}";
            var candidates = new List<string>();
            foreach (var kvp in _docDic)
            {
                if (kvp.Key.EndsWith(langSuffix, StringComparison.Ordinal))
                    candidates.Add(kvp.Key);
            }
            if (candidates.Count == 1)
            {
                _langPairToDocKey[langKey] = candidates[0];
                LoggingHelper.Verbose($"View document ID resolved by language pair scan. RequestedKey={key}, ResolvedKey={candidates[0]}");
                return candidates[0];
            }
            if (candidates.Count > 1)
            {
                LoggingHelper.Warn($"Multiple documents match language pair {srcLang}|{tgtLang}, cannot auto-resolve View key. Keys={string.Join(", ", candidates)}");
            }

            return key;
        }

        #region memoQ 回调

        // 以下回调方法不会被并发调用，会被以队列顺序调用，请勿向外抛出异常，貌似上层不会处理，导致之后的回调异常。

        // 1. 用户在 memoQ 中切换了行, 我们把当前行保留下来, 翻译的时候知道当前行是哪一行
        public void HandleChangeHighlightRequest(ChangeHighlightRequestFromMQ changeHighlighRequest)
        {
            try
            {
                //LoggingHelper.Log($"HandleChangeHighlightRequest: {changeHighlighRequest.ActivePreviewParts.Length}");
                var activeParts = changeHighlighRequest.ActivePreviewParts;
                if (activeParts.Length <= 0) return;

                var firstPart = activeParts.First();
                var lastPart = activeParts.Last();

                string key = GetKey(firstPart);

                foreach (var activePart in activeParts)
                {
                    SetDocName(activePart);
                    SetContext(activePart);
                }

                //if (currentIndexDic.ContainsKey(key))
                //LoggingHelper.Log($"HandleChangeHighlightRequest 修改前：{currentIndexDic[key].IndexStart}, {currentIndexDic[key].IndexEnd}");

                _currentIndexDic[key] = new CurrentIndex()
                {
                    IndexStart = int.Parse(firstPart.PreviewPartId.Split('-').Last()),
                    IndexEnd = int.Parse(lastPart.PreviewPartId.Split('-').Last()),
                    UtcMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };

                //LoggingHelper.Log($"HandleChangeHighlightRequest 修改前：{currentIndexDic[key].IndexStart}, {currentIndexDic[key].IndexEnd}");
            }
            catch (Exception ex)
            {
                ExceptionHandler(ex);
            }
        }

        // 2. 用于获取全部句段的 ID
        public void HandlePreviewPartIdUpdateRequest(PreviewPartIdUpdateRequestFromMQ previewPartIdUpdateRequest)
        {
            try
            {
                // Logging only; memoQ does not provide document identity on this callback, so we do not persist these ids here.
                LoggingHelper.Verbose($"HandlePreviewPartIdUpdateRequest received. Count={previewPartIdUpdateRequest.PreviewPartIds?.Length ?? 0}");
            }
            catch (Exception ex)
            {
                ExceptionHandler(ex);
            }
        }

        // 3. 用于获取全部句段的内容
        public void HandleContentUpdateRequest(ContentUpdateRequestFromMQ contentUpdateRequest)
        {
            try
            {
                //LoggingHelper.Log($"HandleContentUpdateRequest（主动）: {contentUpdateRequest.PreviewParts.Length}");

                foreach (var previewPart in contentUpdateRequest.PreviewParts)
                {
                    SetDocName(previewPart);

                    SetContext(previewPart);
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler(ex);
            }
        }

        // 4. 断开连接
        public void HandleDisconnect()
        {
            try
            {
                //LoggingHelper.Log($"HandleDisconnect");
            }
            catch (Exception ex)
            {
                ExceptionHandler(ex);
            }
        }

        #endregion


        #region 其他

        private void CheckConnect()
        {
            try
            {
                _request.RequestRuntimeSettingsChange(new ChangeRuntimeSettingsRequest(
                    ContentComplexityLevel.Minimal,
                    PropertyNames.SupportedProperties));

                //LoggingHelper.Log("测试连接成功！");
            }
            catch
            {
                try
                {
                    _request.ConnetOrRegister();
                }
                catch (Exception ex)
                {
                    if (ex is PreviewToolAlreadyConnectedException)
                    {
                        // Do Nothing.
                    }
                    else
                    {
                        throw new Exception("The connection with the Preview Helper still fails after reconnecting, please try to restart the software");
                    }
                }

                throw new Exception("The connection with the preview Helper has been disconnected and reconnected successfully. Please reactivate the current segment");
            }
        }

        private void ExceptionHandler(Exception ex)
        {
            LoggingHelper.Warn("preview helper exception message: " + ex.Message);
            LoggingHelper.Warn("preview helper exception stack trace: \r\n" + ex.StackTrace);
        }

        #endregion
    }

    class CurrentIndex
    {
        public int IndexStart { get; set; }

        public int IndexEnd { get; set; }

        public long UtcMs { get; set; }
    }

    class Content
    {
        public string Source { get; set; }

        public string Target { get; set; }

        public IReadOnlyDictionary<string, string> Properties { get; }

        public Content(string source, string target, IReadOnlyDictionary<string, string> properties)
        {
            this.Source = source;
            this.Target = target;
            this.Properties = properties ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

    }

    class MemoqRequest
    {
        private PreviewServiceProxy _proxy = null;

        private readonly ContextHelper _callbackHandler = null;
        private readonly string _baseAddress = "MQ_PREVIEW_PIPE";
        private readonly CommunicationProtocols _communicationProtocol = CommunicationProtocols.NamedPipe;

        private readonly Guid _previewToolId;
        //多供应商机器翻译插件助手
        private readonly string _previewToolName = "Multi Supplier MT Plugin Helper";
        //为多供应商机器翻译插件提供全文文本、全文摘要、上下文功能。
        private readonly string _previewToolDescription;
        private readonly string _autoStartupCommand = " ";
        private readonly string _previewPartIdRegex = ".*";
        private readonly bool _requiresWebPreviewBaseUrl = false;
        private readonly ContentComplexityLevel _contentComplexity = ContentComplexityLevel.Minimal;
        private readonly string[] _requiredProperties = PropertyNames.SupportedProperties;

        public MemoqRequest(ContextHelper callbackHandler, String dllFileName)
        {
            this._callbackHandler = callbackHandler;

            var guidSuffix = BitConverter.ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(dllFileName))).Replace("-", "").ToLower().Substring(0, 12);

            this._previewToolId = Guid.Parse($"c6f2be44-e33c-478e-ba23-{guidSuffix}");

            this._previewToolDescription = $"{dllFileName}, Provides full-text, summary-text, above-text, below-text, and target-text feature for Multi Supplier MT Plugin";
        }

        // 1 创建代理、2 注册、3 连接
        public void ConnetOrRegister()
        {
            if (_proxy == null)
            {
                _proxy = new PreviewServiceProxy(_callbackHandler, _baseAddress, _communicationProtocol);
            }

            var requestStatus = _proxy.Connect(_previewToolId);
            if (!requestStatus.RequestAccepted)
            {
                // NoEnabledPreviewToolWithThisId 可能是未开启，也有可能是未注册，尝试进行注册
                if (requestStatus.ErrorCode == ErrorCodes.NoEnabledPreviewToolWithThisId)
                {
                    var request = new RegistrationRequest(_previewToolId, _previewToolName, _previewToolDescription, _autoStartupCommand,
                                                                _previewPartIdRegex, _requiresWebPreviewBaseUrl, _contentComplexity, _requiredProperties);
                    requestStatus = _proxy.Register(request);
                    if (!requestStatus.RequestAccepted)
                    {
                        throw new ResponseException(requestStatus.ErrorMessage, requestStatus.ErrorCode);
                    }
                }
                else
                {
                    throw new ResponseException(requestStatus.ErrorMessage, requestStatus.ErrorCode);
                }
            }
        }

        // 4 修改文本复杂度
        public void RequestRuntimeSettingsChange(ChangeRuntimeSettingsRequest changeRuntimeSettingsRequest)
        {
            var requestStatus = _proxy.RequestRuntimeSettingsChange(changeRuntimeSettingsRequest);
            if (!requestStatus.RequestAccepted)
            {
                throw new ResponseException(requestStatus.ErrorMessage, requestStatus.ErrorCode);
            }
        }

        // 5 预览 ID 更新
        public void RequestPreviewPartIdUpdate()
        {
            var requestStatus = _proxy.RequestPreviewPartIdUpdate();

            if (!requestStatus.RequestAccepted)
            {
                throw new ResponseException(requestStatus.ErrorMessage, requestStatus.ErrorCode);
            }
        }

        // 6 预览内容更新
        public void RequestContentUpdate(ContentUpdateRequestFromPreviewTool contentUpdateRequest)
        {
            var requestStatus = _proxy.RequestContentUpdate(contentUpdateRequest);

            if (!requestStatus.RequestAccepted)
            {
                throw new ResponseException(requestStatus.ErrorMessage, requestStatus.ErrorCode);
            }
        }

        // 7 改变高亮
        public void RequestHighlightChange(ChangeHighlightRequestFromPreviewTool changeHighlightRequest)
        {
            var requestStatus = _proxy.RequestHighlightChange(changeHighlightRequest);

            if (!requestStatus.RequestAccepted)
            {
                throw new ResponseException(requestStatus.ErrorMessage, requestStatus.ErrorCode);
            }
        }

        // 8 断开连接
        public void Disconnect()
        {
            var requestStatus = _proxy.Disconnect();

            if (!requestStatus.RequestAccepted)
            {
                throw new ResponseException(requestStatus.ErrorMessage, requestStatus.ErrorCode);
            }
        }
    }

    class ResponseException : Exception
    {
        public ErrorCodes? ErrorCode { get; }

        public ResponseException(string message, ErrorCodes? errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}
