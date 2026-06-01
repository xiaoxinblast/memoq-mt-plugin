using MemoQ.Addins.Common.DataStructures;
using MemoQ.Addins.Common.Utils;
using MemoQ.MTInterfaces;
using MultiSupplierMTPlugin.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LLH = MultiSupplierMTPlugin.Localized.LocalizedHelper;
using LLK = MultiSupplierMTPlugin.Localized.LocalizedKeyCommon;

namespace MultiSupplierMTPlugin
{
#if COMPATIBLE_OLD_VERSION
    public class MTRequestMetadata
    {
        public string PorjectID { get; set; } = String.Empty;

        public string Client { get; set; } = String.Empty;

        public string Domain { get; set; } = String.Empty;

        public string Subject { get; set; } = String.Empty;

        public Guid DocumentID { get; set; } = Guid.Empty;

        public Guid ProjectGuid { get; set; } = Guid.Empty;

        public List<SegmentMetadata> SegmentLevelMetadata { get; set; } = new List<SegmentMetadata>();
    }

    public class SegmentMetadata
    {
        public Guid SegmentID { get; set; } = Guid.Empty;

        public ushort SegmentStatus { get; set; } = 0;

        public int SegmentIndex { get; set; } = 0;
    }
    class MultiSupplierMTSession : ISession, ISessionForStoringTranslations
#else
    class MultiSupplierMTSession : ISessionWithMetadata, ISessionForStoringTranslations
#endif
    {
        private readonly MultiSupplierMTGeneralSettings _mtGeneralSettings;
        private readonly MultiSupplierMTSecureSettings _mtSecureSettings;

        private readonly LimitHelper _limitHelper;
        private readonly RetryHelper _retryHelper;

        private readonly MultiSupplierMTService _providerService;
        private readonly RequestType _requestType;

        private readonly string _srcLangCode;
        private readonly string _trgLangCode;

        public MultiSupplierMTSession(MultiSupplierMTOptions mtOptions, LimitHelper limitHelper, RetryHelper retryHelper,
            MultiSupplierMTService providerService, RequestType requestType, string srcLangCode, string trgLangCode)
        {
            this._mtGeneralSettings = mtOptions.GeneralSettings;
            this._mtSecureSettings = mtOptions.SecureSettings;

            this._limitHelper = limitHelper;
            this._retryHelper = retryHelper;

            this._providerService = providerService;
            this._requestType = requestType;

            this._srcLangCode = srcLangCode;
            this._trgLangCode = trgLangCode;
        }

        #region ISessionWithMetadata Members

        public TranslationResult TranslateCorrectSegment(Segment segm, Segment tmSource, Segment tmTarget)
        {
            return TranslateCorrectSegment(segm, tmSource, tmTarget, null);
        }

        public TranslationResult[] TranslateCorrectSegment(Segment[] segs, Segment[] tmSources, Segment[] tmTargets)
        {
            return TranslateCorrectSegment(segs, tmSources, tmTargets, null);
        }

        public TranslationResult TranslateCorrectSegment(Segment segm, Segment tmSource, Segment tmTarget, MTRequestMetadata metaData)
        {
            return TranslateCorrectSegment(new Segment[] { segm }, new Segment[] { tmSource }, new Segment[] { tmTarget }, metaData)[0];
        }

        public TranslationResult[] TranslateCorrectSegment(Segment[] srcSegms, Segment[] tmSrcSegms, Segment[] tmTgtSegms, MTRequestMetadata metaData)
        {
            using (LoggingHelper.BeginRequest())
            {
                LoggingHelper.Info($"TranslateCorrectSegment | Segments={srcSegms?.Length ?? 0} · {_srcLangCode}→{_trgLangCode} · Provider={_providerService.UniqueName}");

            var stopwatch = Stopwatch.StartNew();

            //memoQ 10.0 之前的版本不支持这两个参数
            var hasTm = tmSrcSegms != null && tmTgtSegms != null;

            DumpSourceTags(srcSegms);
            var sourceTagMaps = srcSegms.Select(BuildSourceTagMap).ToList();
            var targetTextNormalizationPlans = srcSegms.Select(BuildTargetTextNormalizationPlan).ToList();

            //记录未翻译文本在原始列表中的位置，翻译后才能将结果放入原始位置
            var untransOriginalIndices = new List<int>();  
            
            var untransSrcTexts = new List<string>();                  //未翻译的句段文本列表（可能包含标记或标签）
            var untransSrcPlainTexts = new List<string>();             //未翻译的句段纯文本列表（不包含标记或标签，用于术语查找）
            var untransSourceTagMaps = new List<string>();             //未翻译句段的源文 tag map
            var untransTargetTextNormalizationPlans = new List<PromptHelper.TargetTextNormalizationPlan>(); // 未翻译句段的现有译文 tag 归一化规则
            var untransCacheSourceTexts = new List<string>();          //包含 tag map 的缓存键文本，避免旧缓存复用错误结果

            var untransTmSrcTexts = hasTm ? new List<string>() : null; //未翻译句段关联的翻译记忆原文纯文本列表
            var untransTmTgtTexts = hasTm ? new List<string>() : null; //未翻译句段关联的翻译记忆译文纯文本列表

            //最终翻译结果列表
            TranslationResult[] results = new TranslationResult[srcSegms.Length];

            //将句段分成两部分（同时转换成纯文本）：缓存中存在的转换到结果列表，缓存中未存在的转换到未翻译列表
            DivideCachedAndUncached(srcSegms, tmSrcSegms, tmTgtSegms, sourceTagMaps, targetTextNormalizationPlans, untransSrcTexts, untransSrcPlainTexts, untransSourceTagMaps, untransTargetTextNormalizationPlans, untransCacheSourceTexts, untransTmSrcTexts, untransTmTgtTexts, untransOriginalIndices, results);

            LoggingHelper.Verbose($"Session request started. Provider={_providerService.UniqueName}, RequestType={_requestType}, SourceLang={_srcLangCode}, TargetLang={_trgLangCode}, SegmentCount={srcSegms.Length}, HasTm={hasTm}, CacheEnabled={_mtGeneralSettings.EnableCache}, CacheHitCount={srcSegms.Length - untransSrcTexts.Count}, CacheMissCount={untransSrcTexts.Count}");

            //翻译缓存中未存在的
            if (untransSrcTexts.Count > 0)
            {
                ProcessUncachedTranslations(srcSegms, untransSrcTexts, untransSrcPlainTexts, untransSourceTagMaps, untransTargetTextNormalizationPlans, untransCacheSourceTexts, untransTmSrcTexts, untransTmTgtTexts, metaData, untransOriginalIndices, results);
            }

            stopwatch.Stop();
            LoggingHelper.Info($"Session finished | Elapsed={stopwatch.ElapsedMilliseconds}ms | Total={srcSegms.Length} | Uncached={untransSrcTexts.Count}");
            LoggingHelper.Separator();

            return results;
            } // End using LoggingHelper.BeginRequest()
        }

        #endregion

        #region Helper Function 1

        // 将句段分成两部分（同时转换成纯文本）：缓存中存在的转换到结果列表，缓存中未存在的转换到未翻译列表
        private void DivideCachedAndUncached(
            Segment[] srcSegms, Segment[] tmSrcSegms, Segment[] tmTgtSegms,
            List<string> sourceTagMaps, List<PromptHelper.TargetTextNormalizationPlan> targetTextNormalizationPlans,
            List<string> untransSrcTexts, List<string> untransSrcPlainTexts,
            List<string> untransSourceTagMaps, List<PromptHelper.TargetTextNormalizationPlan> untransTargetTextNormalizationPlans, List<string> untransCacheSourceTexts,
            List<string> untransTmSrcTexts, List<string> untransTmTgtTexts,
            List<int> untransOriginalIndices, TranslationResult[] results)
        {
            bool hasTm = tmSrcSegms != null && tmTgtSegms != null;
            List<string> srcTexts = srcSegms.Select(ConvertSegment2String).ToList();
            int cacheHitCount = 0;

            for (int i = 0; i < srcTexts.Count; i++)
            {
                var cacheSourceText = BuildCacheSourceText(srcTexts[i], sourceTagMaps[i]);
                var inCache = CacheHelper.TryGet(_providerService.UniqueName, _requestType.ToString(), _srcLangCode, _trgLangCode, cacheSourceText, out string cachedTgtText);

                if (_mtGeneralSettings.EnableCache && inCache && !string.IsNullOrWhiteSpace(cachedTgtText))
                {
                    cacheHitCount++;
                    results[i] = new TranslationResult { Translation = ConvertString2Segment(srcSegms[i], cachedTgtText) };
                }
                else
                {
                    untransOriginalIndices.Add(i);

                    untransSrcTexts.Add(srcTexts[i]);
                    untransSrcPlainTexts.Add(srcSegms[i].PlainText);
                    untransSourceTagMaps.Add(sourceTagMaps[i]);
                    untransTargetTextNormalizationPlans.Add(targetTextNormalizationPlans[i]);
                    untransCacheSourceTexts.Add(cacheSourceText);
                    
                    if (hasTm)
                    {
                        untransTmSrcTexts?.Add(tmSrcSegms[i] != null ? ConvertSegment2String(tmSrcSegms[i]) : "");
                        untransTmTgtTexts?.Add(tmTgtSegms[i] != null ? ConvertSegment2String(tmTgtSegms[i]) : "");
                    }
                }
            }

            LoggingHelper.Verbose($"Cache check finished. Provider={_providerService.UniqueName}, Total={srcTexts.Count}, Hit={cacheHitCount}, Miss={untransSrcTexts.Count}, CacheEnabled={_mtGeneralSettings.EnableCache}");
        }

        // 主翻译逻辑
        private void ProcessUncachedTranslations(
            Segment[] srcSegms,
            List<string> untransSrcTexts, List<string> untransSrcPlainTexts,
            List<string> untransSourceTagMaps, List<PromptHelper.TargetTextNormalizationPlan> untransTargetTextNormalizationPlans, List<string> untransCacheSourceTexts,
            List<string> untransTmSrcTexts, List<string> untransTmTgtTexts, MTRequestMetadata metaData,
            List<int> untransOriginalIndices, TranslationResult[] results)
        {
            var tasks = new List<Task>();
            var batches = splitIntoBatches(untransSrcTexts);

            LoggingHelper.Verbose($"Uncached translation batch plan. Provider={_providerService.UniqueName}, UncachedSegments={untransSrcTexts.Count}, BatchCount={batches.Count}, BatchSizes={string.Join(",", batches.Select(b => b.Count))}, BatchSupported={_providerService.IsBatchSupported}");

            foreach (var (startIndex, count) in batches)
            {
                var batchSrcTexts = untransSrcTexts.Skip(startIndex).Take(count).ToList();
                var batchSrcPlainTexts = untransSrcPlainTexts.Skip(startIndex).Take(count).ToList();
                var batchSourceTagMaps = untransSourceTagMaps.Skip(startIndex).Take(count).ToList();
                var batchTargetTextNormalizationPlans = untransTargetTextNormalizationPlans.Skip(startIndex).Take(count).ToList();
                var batchCacheSourceTexts = untransCacheSourceTexts.Skip(startIndex).Take(count).ToList();
                var batchTmSrcTexts = untransTmSrcTexts?.Skip(startIndex).Take(count).ToList();
                var batchTmTgtTexts = untransTmTgtTexts?.Skip(startIndex).Take(count).ToList();
                var capturedStartIndex = startIndex;
                var capturedCount = count;

                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        LoggingHelper.Verbose($"Batch translation started. Provider={_providerService.UniqueName}, StartIndex={capturedStartIndex}, Count={capturedCount}, CharacterCount={batchSrcTexts.Sum(t => t?.Length ?? 0)}");

                        var batchTgtTexts = await TranslateCoreAsync(batchSrcTexts, batchSrcPlainTexts, batchSourceTagMaps, batchTargetTextNormalizationPlans, batchTmSrcTexts, batchTmTgtTexts, metaData);

                        LoggingHelper.Verbose($"Batch translation returned. Provider={_providerService.UniqueName}, StartIndex={capturedStartIndex}, RequestedCount={capturedCount}, ReturnedCount={batchTgtTexts?.Count ?? 0}");

                        for (int i = 0; i < batchSrcTexts.Count; i++)
                        {
                            int untransIndex = capturedStartIndex + i;                // 在未翻译列表中的索引
                            int originalIndex = untransOriginalIndices[untransIndex]; // 在原始列表中的索引

                            var srcSegm = srcSegms[originalIndex];
                            var srcText = batchSrcTexts[i];
                            var cacheSourceText = batchCacheSourceTexts[i];
                            var tgtText = batchTgtTexts[i];

                            results[originalIndex] = new TranslationResult();
                            try
                            {
                                if (!string.IsNullOrWhiteSpace(srcText) && string.IsNullOrWhiteSpace(tgtText))
                                    throw new Exception("Provider returned empty translation content.");

                                results[originalIndex].Translation = ConvertString2Segment(srcSegm, tgtText);

                                if (_mtGeneralSettings.EnableCache)
                                {
                                    CacheHelper.Store(_providerService.UniqueName, _requestType.ToString(), _srcLangCode, _trgLangCode, cacheSourceText, tgtText);
                                    LoggingHelper.Verbose($"Translation stored in cache. Provider={_providerService.UniqueName}, OriginalIndex={originalIndex}, SourceLength={srcText?.Length ?? 0}, TargetLength={tgtText?.Length ?? 0}");
                                }
                            }
                            catch (Exception ex)
                            {
                                LoggingHelper.Verbose($"Segment post-processing failed. Provider={_providerService.UniqueName}, OriginalIndex={originalIndex}, {ex.GetType().Name}: {ex.Message}");
                                SetSingleExecption(results, originalIndex, ex);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggingHelper.Verbose($"Batch translation failed. Provider={_providerService.UniqueName}, StartIndex={capturedStartIndex}, Count={capturedCount}, {ex.GetType().Name}: {ex.Message}");
                        SetBatchException(results, untransOriginalIndices, capturedStartIndex, capturedCount, ex);
                    }
                }));
            }

            Task.WhenAll(tasks).GetAwaiter().GetResult();
        }

        // 大小限制（句段、字符限制）
        private List<(int StartIndex, int Count)> splitIntoBatches(List<string> untransTexts)
        {
            var batches = new List<(int StartIndex, int Count)>();

            // 不支持批量翻译，总是忽略句段限制和字符限制，逐个句段翻译
            if (!_providerService.IsBatchSupported)
            {
                for (int i = 0; i < untransTexts.Count; i++)
                {
                    batches.Add((i, 1));
                }

                LoggingHelper.Verbose($"Batch split finished. Provider={_providerService.UniqueName}, Reason=BatchNotSupported, BatchCount={batches.Count}");
                return batches;
            }

            int maxSegments = _mtGeneralSettings.EnableCustomRequestLimit
                ? _mtGeneralSettings.MaxSegmentsPerRequest
                : _providerService.MaxSegments;

            int maxCharacters = _mtGeneralSettings.EnableCustomRequestLimit
                ? _mtGeneralSettings.MaxCharactersPerRequest
                : _providerService.MaxCharacters;

            bool limitSegments = maxSegments > 0;
            bool limitCharacters = maxCharacters > 0;

            int startIndex = 0;
            while (startIndex < untransTexts.Count)
            {
                int segmCount = 0;
                int charCount = 0;

                for (int i = startIndex; i < untransTexts.Count; i++)
                {
                    int nextLength = untransTexts[i]?.Length ?? 0;

                    // 总是确保有一个句段，无论句段限制、字符限制是多少
                    bool isNotFirstSegment = segmCount > 0;

                    bool wouldExceedSegmentLimit = limitSegments && (segmCount + 1) > maxSegments;
                    bool wouldExceedCharLimit = limitCharacters && (charCount + nextLength) > maxCharacters;

                    if (isNotFirstSegment && (wouldExceedSegmentLimit || wouldExceedCharLimit))
                        break;

                    segmCount++;
                    charCount += nextLength;
                }

                batches.Add((startIndex, segmCount));
                startIndex += segmCount;
            }

            LoggingHelper.Verbose($"Batch split finished. Provider={_providerService.UniqueName}, MaxSegments={maxSegments}, MaxCharacters={maxCharacters}, LimitSegments={limitSegments}, LimitCharacters={limitCharacters}, BatchCount={batches.Count}");

            return batches;
        }

        // 并发限制、速率限制、重试限制
        private async Task<List<string>> TranslateCoreAsync(List<string> batchTexts, List<string> batchPlainTexts, List<string> batchSourceTagMaps, List<PromptHelper.TargetTextNormalizationPlan> batchTargetTextNormalizationPlans, List<string> tmSources, List<string> tmTargets, MTRequestMetadata metaData)
        {
            // 重试限制放在外部缺点：请求还没真正发起，就可能会超时失败
            // 重试限制放在内部缺点：失败重试时，并发限制、速率限制不再起作用
            // TODO：拆分超时和重试限制
            // TOOD：失败重试时，也要受到并发限制、速率限制
            // TODO: 重新实现生产者消费者模型限流，而不是直接就启动一个线程空转等待
            try
            {
                LoggingHelper.Verbose($"Concurrency wait started. Provider={_providerService.UniqueName}, BatchSize={batchTexts.Count}");
                await _limitHelper.ThreadHoldWaitting();
                LoggingHelper.Verbose($"Concurrency hold acquired. Provider={_providerService.UniqueName}, BatchSize={batchTexts.Count}");

                int waittingMs;
                while ((waittingMs = _limitHelper.GetRateWaittingMs()) > 0)
                {
                    LoggingHelper.Verbose($"Rate limit wait. Provider={_providerService.UniqueName}, WaitMs={waittingMs}, BatchSize={batchTexts.Count}");
                    await Task.Delay(waittingMs);
                }

                LoggingHelper.Verbose($"Provider request dispatching. Provider={_providerService.UniqueName}, BatchSize={batchTexts.Count}, SourceChars={batchTexts.Sum(t => t?.Length ?? 0)}, SourcePlainChars={batchPlainTexts.Sum(t => t?.Length ?? 0)}, HasTm={tmSources != null && tmTargets != null}");

                return await _retryHelper.ExecWithRetryAsync(async (cToken) =>
                {
                    List<string> result;
                    try
                    {
                        using (PromptHelper.UseSourceTagMaps(batchSourceTagMaps, batchTargetTextNormalizationPlans))
                        {
                            result = await _providerService.TranslateAsync(batchTexts, batchPlainTexts, _srcLangCode, _trgLangCode, tmSources, tmTargets, metaData, cToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (_mtGeneralSettings.EnableStatsAndLog) StatsHelper.IncrementRequestFailed();

                        LoggingHelper.Verbose($"Provider request failed. Provider={_providerService.UniqueName}, BatchSize={batchTexts.Count}, {ex.GetType().Name}: {ex.Message}");

                        throw ex;
                    }

                    if (_mtGeneralSettings.EnableStatsAndLog) StatsHelper.IncrementRequestSuccess();

                    LoggingHelper.Verbose($"Provider request succeeded. Provider={_providerService.UniqueName}, BatchSize={batchTexts.Count}, ReturnedCount={result?.Count ?? 0}");

                    return result;
                });
            }
            finally
            {
                _limitHelper.ThreadHoldRelease();
                LoggingHelper.Verbose($"Concurrency hold released. Provider={_providerService.UniqueName}, BatchSize={batchTexts.Count}");
            }
        }
        #endregion

        #region Helper Function 2
        private void SetSingleExecption(TranslationResult[] results, int originalIndex, Exception ex)
        {
            string msg = LLH.G(LLK.MultiSupplierMTSession_String2SegmentFail);
            results[originalIndex].Exception = new MTException(msg, msg, ex);
            LoggingHelper.Warn(msg);
        }

        private void SetBatchException(TranslationResult[] results, List<int> untransOriginalIndices, int BatchStartIndex, int BatchCount, Exception ex)
        {
            var msgBuilder = new StringBuilder();
            msgBuilder.AppendLine(LLH.G(LLK.MultiSupplierMTSession_AllSegmentsTranslateFail, BatchCount));
            msgBuilder.AppendLine("\t" + ex.Message);

            if (ex is AggregateException agEx)
            {
                foreach (var inner in agEx.InnerExceptions)
                    msgBuilder.AppendLine("\t\t" + inner.Message);
            }

            string finalMsg = msgBuilder.ToString().TrimEnd();

            for (int i = 0; i < BatchCount; i++)
            {
                int untransIndex = BatchStartIndex + i;
                int originalIndex = untransOriginalIndices[untransIndex];

                results[originalIndex] = new TranslationResult
                {
                    Exception = new MTException(finalMsg, finalMsg, ex)
                };
            }

            LoggingHelper.Warn(finalMsg);
        }

        private string BuildSourceTagMap(Segment segment)
        {
            if (!ShouldBuildSourceTagMap() || segment?.ITags == null || segment.ITags.Length == 0)
                return string.Empty;

            var builder = new StringBuilder();
            for (int tagIndex = 0; tagIndex < segment.ITags.Length; tagIndex++)
            {
                var tag = segment.ITags[tagIndex];
                var meaning = BuildTagMeaning(tag);
                if (!string.IsNullOrWhiteSpace(meaning))
                    builder.AppendLine($"Tag {BuildTagToken(tagIndex)} ; meaning {meaning}");
            }

            return builder.ToString().TrimEnd();
        }

        private PromptHelper.TargetTextNormalizationPlan BuildTargetTextNormalizationPlan(Segment segment)
        {
            var plan = new PromptHelper.TargetTextNormalizationPlan();

            if (!ShouldBuildSourceTagMap() || segment?.ITags == null || segment.ITags.Length == 0)
                return plan;

            for (int tagIndex = 0; tagIndex < segment.ITags.Length; tagIndex++)
            {
                var tag = segment.ITags[tagIndex];
                var rule = new PromptHelper.TargetTextNormalizationRule()
                {
                    Token = BuildTagToken(tagIndex),
                    IsLineBreak = IsLineBreakTag(tag)
                };

                if (!rule.IsLineBreak)
                {
                    AddNormalizationCandidate(rule, ReadTagAttributeRaw(tag, "displaytext"));
                    AddNormalizationCandidate(rule, ReadTagAttributeRaw(tag, "val"));
                }

                plan.Rules.Add(rule);
            }

            return plan;
        }

        private bool ShouldBuildSourceTagMap()
        {
            return _requestType == RequestType.BothFormattingAndTagsWithXml ||
                   _requestType == RequestType.BothFormattingAndTagsWithHtml;
        }

        private string BuildTagToken(int tagIndex)
        {
            if (_requestType == RequestType.BothFormattingAndTagsWithHtml)
                return $"<span data-mqitag=\"{tagIndex}\"></span>";

            return $"<inline_tag id=\"{tagIndex}\"/>";
        }

        private string BuildTagMeaning(InlineTag tag)
        {
            var tagName = SafeReadRaw(() => tag.Name);

            if (string.Equals(tagName, "mq:ch", StringComparison.OrdinalIgnoreCase))
                return "[line-break]";

            var displayText = ReadTagAttribute(tag, "displaytext");
            if (!string.IsNullOrWhiteSpace(displayText))
                return FormatTagMeaning(displayText);

            var value = ReadTagAttribute(tag, "val");
            if (!string.IsNullOrWhiteSpace(value))
                return FormatTagMeaning(value);

            return string.IsNullOrWhiteSpace(tagName)
                ? $"memoQ tag ({SafeReadRaw(() => tag.TagType)})"
                : $"memoQ tag ({tagName}, {SafeReadRaw(() => tag.TagType)})";
        }

        private static string ReadTagAttribute(InlineTag tag, string attributeName)
        {
            try
            {
                foreach (var attr in tag.Attributes)
                {
                    var name = SafeReadRaw(() => attr.Name);
                    if (string.Equals(name, attributeName, StringComparison.OrdinalIgnoreCase))
                        return SafeReadRaw(() => attr.ValueString);
                }
            }
            catch
            {
                return string.Empty;
            }

            return string.Empty;
        }

        private static string FormatTagMeaning(string value)
        {
            var meaning = EscapeControlChars(value).Trim();
            if (string.IsNullOrWhiteSpace(meaning))
                return string.Empty;

            if (meaning.StartsWith("<") && meaning.EndsWith(">") && meaning.Length > 2)
                meaning = meaning.Substring(1, meaning.Length - 2).Trim();

            meaning = meaning
                .Replace("<", "[")
                .Replace(">", "]")
                .Replace("\"", "'");

            if (meaning.StartsWith("ICON ", StringComparison.OrdinalIgnoreCase))
                return "[icon: " + meaning.Substring(5).Trim() + "]";

            if (meaning.StartsWith("REF ", StringComparison.OrdinalIgnoreCase))
                return "[reference: " + meaning.Substring(4).Trim() + "]";

            return "[" + meaning + "]";
        }

        private static string BuildCacheSourceText(string sourceText, string sourceTagMap)
        {
            if (string.IsNullOrWhiteSpace(sourceTagMap))
                return sourceText;

            return sourceText + Environment.NewLine + Environment.NewLine + "[memoQ Source Tag Map]" + Environment.NewLine + sourceTagMap;
        }

        private void DumpSourceTags(Segment[] srcSegms)
        {
            if (!LoggingHelper.Enable || !LoggingHelper.EnableVerboseRuntimeLog)
                return;

            try
            {
                var msgBuilder = new StringBuilder();
                msgBuilder.AppendLine("Source Tag Dump");
                msgBuilder.AppendLine($"Provider: {_providerService.UniqueName}");
                msgBuilder.AppendLine($"RequestType: {_requestType}");
                msgBuilder.AppendLine($"SourceLang: {_srcLangCode}");
                msgBuilder.AppendLine($"TargetLang: {_trgLangCode}");
                msgBuilder.AppendLine($"SegmentCount: {srcSegms?.Length ?? 0}");

                for (int segmIndex = 0; segmIndex < (srcSegms?.Length ?? 0); segmIndex++)
                {
                    var segment = srcSegms[segmIndex];
                    var tags = segment?.ITags ?? new InlineTag[0];

                    msgBuilder.AppendLine($"SegmentIndex: {segmIndex}");
                    msgBuilder.AppendLine($"PlainText: {segment?.PlainText ?? string.Empty}");
                    msgBuilder.AppendLine($"SourceTextSentToAI: {TryConvertSegment2String(segment)}");
                    msgBuilder.AppendLine($"ITags.Count: {tags.Length}");

                    for (int tagIndex = 0; tagIndex < tags.Length; tagIndex++)
                    {
                        var tag = tags[tagIndex];
                        msgBuilder.AppendLine($"Tag[{tagIndex}]: TagType={SafeRead(() => tag.TagType)}, Name={SafeRead(() => tag.Name)}, AttrCount={SafeRead(() => tag.AttrCount)}");

                        int attrIndex = 0;
                        foreach (var attr in tag.Attributes)
                        {
                            msgBuilder.AppendLine($"  Attr[{attrIndex}]: " +
                                $"Name={SafeRead(() => attr.Name)}, " +
                                $"ValueString={SafeRead(() => attr.ValueString)}, " +
                                $"ValueRowGuid={SafeRead(() => attr.ValueRowGuid)}, " +
                                $"IsTranslatable={SafeRead(() => attr.IsTranslatable)}, " +
                                $"IsSeparateRowTranslatable={SafeRead(() => attr.IsSeparateRowTranslatable)}, " +
                                $"IsSelfContainedTranslatable={SafeRead(() => attr.IsSelfContainedTranslatable)}");
                            attrIndex++;
                        }
                    }
                }

                LoggingHelper.Verbose(msgBuilder.ToString().TrimEnd());
            }
            catch (Exception ex)
            {
                LoggingHelper.Verbose($"Source Tag Dump failed. {ex.GetType().Name}: {ex.Message}");
            }
        }

        private static string SafeRead(Func<object> read)
        {
            try
            {
                return EscapeControlChars(read()?.ToString() ?? string.Empty);
            }
            catch (Exception ex)
            {
                return $"<unavailable: {ex.GetType().Name}: {ex.Message}>";
            }
        }

        private static string SafeReadRaw(Func<object> read)
        {
            try
            {
                return read()?.ToString() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string EscapeControlChars(string value)
        {
            if (value == null)
                return string.Empty;

            var builder = new StringBuilder();
            foreach (var ch in value)
            {
                switch (ch)
                {
                    case '\r':
                        builder.Append("\\r");
                        break;
                    case '\n':
                        builder.Append("\\n");
                        break;
                    case '\t':
                        builder.Append("\\t");
                        break;
                    default:
                        if (char.IsControl(ch))
                            builder.Append("\\u" + ((int)ch).ToString("X4"));
                        else
                            builder.Append(ch);
                        break;
                }
            }

            return builder.ToString();
        }

        private string TryConvertSegment2String(Segment segment)
        {
            try
            {
                return segment == null ? string.Empty : ConvertSegment2String(segment);
            }
            catch (Exception ex)
            {
                return $"<failed to convert source segment: {ex.GetType().Name}: {ex.Message}>";
            }
        }

        private string ConvertSegment2String(Segment segment)
        {
            switch (_requestType)
            {
                case RequestType.OnlyFormattingWithXml:
                    return SegmentXMLConverter.ConvertSegment2Xml(segment, false, true);
                case RequestType.OnlyFormattingWithHtml:
                    return SegmentHtmlConverter.ConvertSegment2Html(segment, false);
                case RequestType.BothFormattingAndTagsWithXml:
                    return SegmentXMLConverter.ConvertSegment2Xml(segment, true, true);
                case RequestType.BothFormattingAndTagsWithHtml:
                    return SegmentHtmlConverter.ConvertSegment2Html(segment, true);
                default:
                    return segment.PlainText;
            }
        }

        private Segment ConvertString2Segment(Segment originalSegment, string translatedText)
        {
            translatedText = NormalizeDisplayedTagsToSourceTokens(originalSegment, translatedText);

            Segment segment;
            if (_requestType == RequestType.OnlyFormattingWithXml || _requestType == RequestType.BothFormattingAndTagsWithXml)
            {
                segment = SegmentXMLConverter.ConvertXML2Segment(translatedText, originalSegment.ITags);
            }
            else if (_requestType == RequestType.OnlyFormattingWithHtml || _requestType == RequestType.BothFormattingAndTagsWithHtml)
            {
                segment = SegmentHtmlConverter.ConvertHtml2Segment(translatedText, originalSegment.ITags);
            }
            else
            {
                segment = SegmentBuilder.CreateFromString(translatedText);
            }

            if (_requestType == RequestType.BothFormattingAndTagsWithXml || _requestType == RequestType.BothFormattingAndTagsWithHtml)
            {
                if (_mtGeneralSettings.NormalizeWhitespaceAroundTags)
                {
#if COMPATIBLE_OLD_VERSION
                    LoggingHelper.Warn("your memoq version is lower than 9.14 and does not support Normalize Whitespace Around Tags");
#else
                    segment = TagWhitespaceNormalizer.NormalizeWhitespaceAroundTags(originalSegment, segment, this._srcLangCode, this._trgLangCode);                    
#endif
                }
            }
            else
            {
                if (_mtGeneralSettings.InsertRequiredTagsToEnd)
                {
                    SegmentBuilder sb = new SegmentBuilder();
                    sb.AppendSegment(segment);

                    foreach (InlineTag it in originalSegment.ITags)
                        sb.AppendInlineTag(it);

                    segment = sb.ToSegment();
                }
            }

            return segment;
        }

        private string NormalizeDisplayedTagsToSourceTokens(Segment originalSegment, string translatedText)
        {
            if (!ShouldBuildSourceTagMap() || originalSegment?.ITags == null || string.IsNullOrEmpty(translatedText))
                return translatedText;

            var result = translatedText;
            for (int tagIndex = 0; tagIndex < originalSegment.ITags.Length; tagIndex++)
            {
                var tag = originalSegment.ITags[tagIndex];
                var token = BuildTagToken(tagIndex);

                if (IsLineBreakTag(tag))
                {
                    result = ReplaceFirstAny(result, token, "<br/>", "<br />", "<br>");
                    continue;
                }

                var displayText = ReadTagAttributeRaw(tag, "displaytext");
                if (!string.IsNullOrWhiteSpace(displayText))
                    result = ReplaceFirst(result, displayText, token);

                var value = ReadTagAttributeRaw(tag, "val");
                if (!string.IsNullOrWhiteSpace(value))
                    result = ReplaceFirst(result, value, token);
            }

            return result;
        }

        private static bool IsLineBreakTag(InlineTag tag)
        {
            return string.Equals(SafeReadRaw(() => tag.Name), "mq:ch", StringComparison.OrdinalIgnoreCase);
        }

        private static string ReadTagAttributeRaw(InlineTag tag, string attributeName)
        {
            try
            {
                foreach (var attr in tag.Attributes)
                {
                    var name = SafeReadRaw(() => attr.Name);
                    if (string.Equals(name, attributeName, StringComparison.OrdinalIgnoreCase))
                        return SafeReadRaw(() => attr.ValueString);
                }
            }
            catch
            {
                return string.Empty;
            }

            return string.Empty;
        }

        private static void AddNormalizationCandidate(PromptHelper.TargetTextNormalizationRule rule, string candidate)
        {
            if (rule == null || string.IsNullOrWhiteSpace(candidate))
                return;

            foreach (var existing in rule.Candidates)
            {
                if (string.Equals(existing, candidate, StringComparison.Ordinal))
                    return;
            }

            rule.Candidates.Add(candidate);
        }

        private static string ReplaceFirstAny(string text, string replacement, params string[] candidates)
        {
            foreach (var candidate in candidates)
            {
                var replaced = ReplaceFirst(text, candidate, replacement);
                if (!string.Equals(replaced, text, StringComparison.Ordinal))
                    return replaced;
            }

            return text;
        }

        private static string ReplaceFirst(string text, string oldValue, string newValue)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(oldValue))
                return text;

            var index = text.IndexOf(oldValue, StringComparison.Ordinal);
            if (index < 0)
                return text;

            return text.Substring(0, index) + newValue + text.Substring(index + oldValue.Length);
        }

        #endregion

        #region ISessionForStoringTranslations

        public void StoreTranslation(TranslationUnit transunit)
        {
            StoreTranslation(new TranslationUnit[] { transunit });
        }

        public int[] StoreTranslation(TranslationUnit[] transunits)
        {
            int[] stored = new int[transunits.Length];
            for (int i = 0; i < transunits.Length; i++)
            {
                try
                {
                    CacheHelper.Store(_providerService.UniqueName, _requestType.ToString(), _srcLangCode, _trgLangCode,
                        ConvertSegment2String(transunits[i].Source), ConvertSegment2String(transunits[i].Target));

                    stored[i] = i;
                }
                catch
                {
                    // do nothing
                }
            }
            return stored;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion
    }
}
