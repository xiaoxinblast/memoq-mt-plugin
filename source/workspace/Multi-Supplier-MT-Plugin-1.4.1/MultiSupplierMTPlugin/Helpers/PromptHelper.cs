using MemoQ.MTInterfaces;
using MultiSupplierMTPlugin.ProvidersCommon.Options.LLM;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using LLH = MultiSupplierMTPlugin.Localized.LocalizedHelper;
using LLKC = MultiSupplierMTPlugin.Localized.LocalizedKeyCommon;

namespace MultiSupplierMTPlugin.Helpers
{
    class PromptHelper
    {
        private const string _GLOSSARY_TEXT_KEY = "glossary-text";

        private const string _SOURCE_LANGUAGE_KEY = "source-language";

        private const string _TARGET_LANGUAGE_KEY = "target-language";

        private const string _SOURCE_TEXT_KEY = "source-text";

        private const string _TARGET_TEXT_KEY = "target-text";

        private const string _LINE_LENGTH_LIMIT_KEY = "line-length-limit";

        private const string _CHAR_COUNT_KEY = "char-count";

        private const string _TAG_MAP_KEY = "tag-map";

        private const string _TM_SOURCE_TEXT_KEY = "tm-source-text";

        private const string _TM_TARGET_TEXT_KEY = "tm-target-text";

        private const string _FULL_TEXT_KEY = "full-text";

        private const string _SUAMMARY_TEXT_KEY = "summary-text";

        private const string _ABOVE_TEXT_KEY = "above-text";

        private const string _BELOW_TEXT_KEY = "below-text";

        private static readonly HashSet<string> _KNOWN_PLACEHOLDER_NAMES = new HashSet<string>()
        {
            _GLOSSARY_TEXT_KEY,
            _SOURCE_LANGUAGE_KEY, _TARGET_LANGUAGE_KEY,
            _SOURCE_TEXT_KEY, _TARGET_TEXT_KEY, _LINE_LENGTH_LIMIT_KEY, _CHAR_COUNT_KEY, _TAG_MAP_KEY,
            _TM_SOURCE_TEXT_KEY, _TM_TARGET_TEXT_KEY,
            _FULL_TEXT_KEY, _SUAMMARY_TEXT_KEY,
            _ABOVE_TEXT_KEY, _BELOW_TEXT_KEY
        };

        private static readonly AsyncLocal<List<string>> _sourceTagMaps = new AsyncLocal<List<string>>();

        private static readonly AsyncLocal<List<TargetTextNormalizationPlan>> _targetTextNormalizationPlans = new AsyncLocal<List<TargetTextNormalizationPlan>>();

        private static readonly Regex _sourceTagTokenRegex = new Regex(@"<inline_tag id=""\d+""\s*/>|<span\s+data-mqitag=""\d+"">\s*</span>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex _contextTargetMarkerRegex = new Regex(@"\r\n|\n|\r|<[^>]+>", RegexOptions.Compiled);

        private static readonly HashSet<string> _htmlLikeContextTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "a", "abbr", "b", "br", "code", "div", "em", "font", "i", "img", "li", "ol", "p", "pre", "s", "small", "span", "strong", "sub", "sup", "table", "tbody", "td", "th", "thead", "tr", "u", "ul"
        };


        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

        public static IDisposable UseSourceTagMaps(List<string> sourceTagMaps, List<TargetTextNormalizationPlan> targetTextNormalizationPlans = null)
        {
            var previousSourceTagMaps = _sourceTagMaps.Value;
            var previousTargetTextNormalizationPlans = _targetTextNormalizationPlans.Value;
            _sourceTagMaps.Value = sourceTagMaps;
            _targetTextNormalizationPlans.Value = targetTextNormalizationPlans;
            return new SourceTagMapScope(previousSourceTagMaps, previousTargetTextNormalizationPlans);
        }

        public class TargetTextNormalizationPlan
        {
            public List<TargetTextNormalizationRule> Rules { get; } = new List<TargetTextNormalizationRule>();
        }

        public class TargetTextNormalizationRule
        {
            public string Token { get; set; }

            public bool IsLineBreak { get; set; }

            public List<string> Candidates { get; } = new List<string>();
        }

        public static ContextMenuStrip CreateTextBoxContextMenu()
        {
            var menu = new ContextMenuStrip();

            void Add(string text, Action<TextBoxBase> action)
            {
                var item = new ToolStripMenuItem(text);
                item.Click += (s, e) =>
                {
                    if (menu.SourceControl is TextBoxBase tb)
                        action(tb);
                };
                menu.Items.Add(item);
            }

            void Insert(string label, string text) =>
                Add(label, tb =>
                {
                    if (!tb.Focused) tb.Focus();
                    int EM_REPLACESEL = 0x00C2;
                    SendMessage(tb.Handle, EM_REPLACESEL, (IntPtr)1, text);
                });

            void AddSeparator() => menu.Items.Add(new ToolStripSeparator());

            string Build(string name, bool noEmpty)
            {
                var no = noEmpty ? "!" : "";
                return "{{" + name + no + "}}";
            }

            Insert(LLH.G(LLKC.TextBoxPromptMenu_SourceLanguage), Build(_SOURCE_LANGUAGE_KEY, true));
            Insert(LLH.G(LLKC.TextBoxPromptMenu_TargetLanguage), Build(_TARGET_LANGUAGE_KEY, true));
            AddSeparator();
            Insert(LLH.G(LLKC.TextBoxPromptMenu_SourceText), Build(_SOURCE_TEXT_KEY, true));
            Insert(LLH.G(LLKC.TextBoxPromptMenu_TargetText), Build(_TARGET_TEXT_KEY, true));
            AddSeparator();
            Insert(LLH.G(LLKC.TextBoxPromptMenu_TmSourceText), Build(_TM_SOURCE_TEXT_KEY, false));
            Insert(LLH.G(LLKC.TextBoxPromptMenu_TmTargetText), Build(_TM_TARGET_TEXT_KEY, false));
            AddSeparator();
            Insert(LLH.G(LLKC.TextBoxPromptMenu_AboveText), Build(_ABOVE_TEXT_KEY, false));
            Insert(LLH.G(LLKC.TextBoxPromptMenu_BelowText), Build(_BELOW_TEXT_KEY, false));
            AddSeparator();
            Insert(LLH.G(LLKC.TextBoxPromptMenu_SuammaryText), Build(_SUAMMARY_TEXT_KEY, true));
            Insert(LLH.G(LLKC.TextBoxPromptMenu_FullText), Build(_FULL_TEXT_KEY, true));
            AddSeparator();
            Insert(LLH.G(LLKC.TextBoxPromptMenu_GlossaryText), Build(_GLOSSARY_TEXT_KEY, false));

            return menu;
        }

        
        public static (string, string) Parse(
            string systemPrompt, string userPrompt,

            MultiSupplierMTOptions mtOptions,

            ProviderOptions providerOptions,
            Dictionary<string, string> supportLanguages,
            MultiSupplierMTService service,

            List<string> texts, List<string> plainTexts,
            string srcLang, string tgtLang,
            List<string> tmSources, List<string> tmTargets,
            MTRequestMetadata metaData
            )
        {
            // 解决 xml 反序列化后换行符总是变成 \n
            systemPrompt = systemPrompt.Replace(Environment.NewLine, "\n").Replace("\n", Environment.NewLine);
            userPrompt = userPrompt.Replace(Environment.NewLine, "\n").Replace("\n", Environment.NewLine);

            var cSettings = mtOptions.GeneralSettings.LLMCommon;
            var bSettings = providerOptions.GeneralSettings as LLMBaseGeneralSettings;

            var promptBuilder = new PromptBuilder(systemPrompt, userPrompt, _KNOWN_PLACEHOLDER_NAMES);
            var tagMapSection = BuildTagMapSection(_sourceTagMaps.Value);

            // 术语表
            if (promptBuilder.HasPlaceholder(_GLOSSARY_TEXT_KEY))
            {
                var glsFilePath = cSettings.GlossaryFilePath;
                var glsDelimiter = cSettings.GlossaryDelimiter;

                string glossary = GlossaryHelper.ReadGlossaryString(plainTexts, glsFilePath, srcLang, tgtLang, glsDelimiter, "utf-8", true) ;
                
                promptBuilder.SetPlaceholder(_GLOSSARY_TEXT_KEY, glossary);
            }

            // 源语言
            if (promptBuilder.HasPlaceholder(_SOURCE_LANGUAGE_KEY))
            {
                if (!supportLanguages.ContainsKey(srcLang)) new Exception($"Source language code is not supported: {srcLang}");

                promptBuilder.SetPlaceholder(_SOURCE_LANGUAGE_KEY, supportLanguages[srcLang]);
            }

            // 目标语言
            if (promptBuilder.HasPlaceholder(_TARGET_LANGUAGE_KEY))
            {
                if (!supportLanguages.ContainsKey(tgtLang)) new Exception($"Target language code is not supported: {tgtLang}");

                promptBuilder.SetPlaceholder(_TARGET_LANGUAGE_KEY, supportLanguages[tgtLang]);
            }

            // 源文本
            if (promptBuilder.HasPlaceholder(_SOURCE_TEXT_KEY))
            {
                var sourceText = bSettings.EnableBathTranslate
                        ? BathTranslateHelper.Serialize(bSettings.BathTranslateSchema, texts)
                        : texts[0];

                promptBuilder.SetPlaceholder(_SOURCE_TEXT_KEY, sourceText);
            }

            if (promptBuilder.HasPlaceholder(_TAG_MAP_KEY))
            {
                promptBuilder.SetPlaceholder(_TAG_MAP_KEY, tagMapSection);
            }

            if (promptBuilder.HasPlaceholder(_LINE_LENGTH_LIMIT_KEY))
                promptBuilder.ClearPlaceholder(_LINE_LENGTH_LIMIT_KEY);

            if (promptBuilder.HasPlaceholder(_CHAR_COUNT_KEY))
                promptBuilder.ClearPlaceholder(_CHAR_COUNT_KEY);

            // 目标文本（目前预览 SDK 获取获取目标文本不带有标签，且有的句段（比如图片名字）无法获取）
            //if (promptBuilder.HasPlaceholder(TARGET_TEXT_KEY))
            //{
            //
            //}

            // 源文本（翻译记忆中保存的）
            if (promptBuilder.HasPlaceholder(_TM_SOURCE_TEXT_KEY))
            {
                if (tmSources == null) throw new Exception($"{_TM_SOURCE_TEXT_KEY} Placeholders require memoQ min version 10.0, and enable \"Send best fuzzy TM\" in memoq settings");

                var tmSourceText = bSettings.EnableBathTranslate
                    ? BathTranslateHelper.Serialize(bSettings.BathTranslateSchema, tmSources)
                    : tmSources[0];

                promptBuilder.SetPlaceholder(_TM_SOURCE_TEXT_KEY, tmSourceText);
            }

            // 目标文本（翻译记忆中保存的）
            if (promptBuilder.HasPlaceholder(_TM_TARGET_TEXT_KEY))
            {
                if (tmTargets == null) throw new Exception($"{_TM_TARGET_TEXT_KEY} Placeholders require memoQ min version 10.0, and enable \"Send best fuzzy TM\" in memoq settings");

                var tmTargetText = bSettings.EnableBathTranslate
                    ? BathTranslateHelper.Serialize(bSettings.BathTranslateSchema, tmTargets)
                    : tmTargets[0];

                promptBuilder.SetPlaceholder(_TM_TARGET_TEXT_KEY, tmTargetText);
            }

            if (promptBuilder.HasPlaceholder(_FULL_TEXT_KEY) || promptBuilder.HasPlaceholder(_SUAMMARY_TEXT_KEY) ||
                promptBuilder.HasPlaceholder(_ABOVE_TEXT_KEY) || promptBuilder.HasPlaceholder(_BELOW_TEXT_KEY) ||
                promptBuilder.HasPlaceholder(_TARGET_TEXT_KEY))
            {
                // 全文、摘要、上下文、目标文本需要 memoQ 版本大于 9.14 才能获取到 metaData
                if (metaData == null) throw new Exception($"{_FULL_TEXT_KEY}, {_SUAMMARY_TEXT_KEY}, {_ABOVE_TEXT_KEY}, {_BELOW_TEXT_KEY}, {_TARGET_TEXT_KEY} Placeholders require memoQ min version 9.14");

                var prjGuid = metaData.ProjectGuid.ToString();
                var docGuid = metaData.DocumentID.ToString();

                // 全文文本
                if (promptBuilder.HasPlaceholder(_FULL_TEXT_KEY))
                {
                    string fullText = ContextHelper.Instance.GetFullText(prjGuid, docGuid, srcLang, tgtLang);
                    promptBuilder.SetPlaceholder(_FULL_TEXT_KEY, fullText);
                }

                // 全文摘要
                if (promptBuilder.HasPlaceholder(_SUAMMARY_TEXT_KEY))
                {
                    string summary;

                    if (cSettings.SummaryAutoGenerate)
                    {
                        using (UseSourceTagMaps(null))
                        {
                            summary = SummaryHelper.ReadFromCacheOrGenerate(prjGuid, docGuid, srcLang, tgtLang,
                                    mtOptions, providerOptions, service, texts, plainTexts, tmSources, tmTargets, metaData);
                        }
                    }
                    else
                    {
                        summary = SummaryHelper.ReadFromFile(cSettings.SummaryFilePath);
                    }

                    promptBuilder.SetPlaceholder(_SUAMMARY_TEXT_KEY, summary);
                }

                // 上文、下文、目标文本 需要界面交互
                if (promptBuilder.HasPlaceholder(_ABOVE_TEXT_KEY) || promptBuilder.HasPlaceholder(_BELOW_TEXT_KEY) ||
                    promptBuilder.HasPlaceholder(_TARGET_TEXT_KEY))
                {
                    if (texts.Count > 1) throw new Exception("Batch translation(Pre-translation) does not support getting above-text, below-text or target-text");

                    try
                    {
                        var currentIndex = GetSegmIndex(prjGuid, docGuid, srcLang, tgtLang);
                        //LoggingHelper.Log($"Prompt segmIndex: {currentIndex.IndexStart}, {currentIndex.IndexEnd}");

                        if (promptBuilder.HasPlaceholder(_ABOVE_TEXT_KEY))
                        {
                            var aboveMaxSegm = cSettings.AboveTextMaxSegments;
                            var aboveMaxChar = cSettings.AboveTextMaxCharacters;
                            var aboveIncludeSrc = cSettings.AboveTextIncludeSource;
                            var aboveIncludeTgt = cSettings.AboveTextIncludeTarget;

                            string aboveText = ContextHelper.Instance.GetAboveContext(prjGuid, docGuid, srcLang, tgtLang,
                                currentIndex.IndexStart, aboveMaxSegm, aboveMaxChar, aboveIncludeSrc, aboveIncludeTgt, NormalizeContextTargetTextForPrompt);

                            promptBuilder.SetPlaceholder(_ABOVE_TEXT_KEY, aboveText);
                        }

                        if (promptBuilder.HasPlaceholder(_BELOW_TEXT_KEY))
                        {
                            var belowMaxSegm = cSettings.BelowTextMaxSegments;
                            var belowMaxChar = cSettings.BelowTextMaxCharacters;
                            var belowIncludeSrc = cSettings.BelowTextIncludeSource;
                            var belowIncludeTgt = cSettings.BelowTextIncludeTarget;

                            string belowText = ContextHelper.Instance.GetBelowContext(prjGuid, docGuid, srcLang, tgtLang,
                                currentIndex.IndexEnd, belowMaxSegm, belowMaxChar, belowIncludeSrc, belowIncludeTgt, NormalizeContextTargetTextForPrompt);

                            promptBuilder.SetPlaceholder(_BELOW_TEXT_KEY, belowText);
                        }

                        // 目标文本（目前预览 SDK 获取获取目标文本不带有标签，且有的句段（比如图片名字）无法获取）
                        if (promptBuilder.HasPlaceholder(_TARGET_TEXT_KEY))
                        {
                            string targetText = "";
                            for (int i = currentIndex.IndexStart; i <= currentIndex.IndexEnd; i++)
                            {
                                targetText += ContextHelper.Instance.GetTargetText(prjGuid, docGuid, srcLang, tgtLang, i);
                            }

                            var targetTextNormalizationPlan = GetTargetTextNormalizationPlan();
                            if (targetTextNormalizationPlan != null)
                            {
                                var normalizedTargetText = NormalizeTargetTextForPrompt(targetText, targetTextNormalizationPlan);
                                if (!string.Equals(normalizedTargetText, targetText, StringComparison.Ordinal))
                                    LoggingHelper.Verbose($"Preview Helper target-text normalized to source tag tokens. OriginalChars={targetText?.Length ?? 0}, NormalizedChars={normalizedTargetText?.Length ?? 0}");

                                targetText = normalizedTargetText;
                            }

                            promptBuilder.SetPlaceholder(_TARGET_TEXT_KEY, targetText);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (promptBuilder.HasPlaceholder(_TARGET_TEXT_KEY))
                        {
                            LoggingHelper.Warn($"Preview Helper target-text unavailable; aborting request to avoid sending empty current translation. {ex.GetType().Name}: {ex.Message}");
                            throw new Exception("无法从 memoQ Preview Helper 读取现有译文。请重新激活当前句段后重试。", ex);
                        }

                        LoggingHelper.Warn($"Preview Helper context placeholders unavailable; continuing without above-text or below-text. {ex.GetType().Name}: {ex.Message}");
                        promptBuilder.ClearPlaceholder(_ABOVE_TEXT_KEY);
                        promptBuilder.ClearPlaceholder(_BELOW_TEXT_KEY);
                    }
                }
            }

            var prompts = promptBuilder.BuildPrompts();

            if (!promptBuilder.HasPlaceholder(_TAG_MAP_KEY) && !string.IsNullOrWhiteSpace(tagMapSection))
            {
                prompts.Item2 = AppendSection(prompts.Item2, tagMapSection);
            }

            return prompts;
        }

        private static string BuildTagMapSection(List<string> sourceTagMaps)
        {
            if (sourceTagMaps == null || sourceTagMaps.Count == 0)
                return string.Empty;

            var nonEmptyMaps = new List<(int Index, string Map)>();
            for (int i = 0; i < sourceTagMaps.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(sourceTagMaps[i]))
                    nonEmptyMaps.Add((i, sourceTagMaps[i]));
            }

            if (nonEmptyMaps.Count == 0)
                return string.Empty;

            var builder = new StringBuilder();
            builder.AppendLine("Tag information:");

            if (sourceTagMaps.Count == 1)
            {
                builder.AppendLine(nonEmptyMaps[0].Map);
            }
            else
            {
                foreach (var item in nonEmptyMaps)
                {
                    builder.AppendLine($"Segment {item.Index + 1}:");
                    builder.AppendLine(item.Map);
                }
            }

            return builder.ToString().TrimEnd();
        }

        private static string AppendSection(string prompt, string section)
        {
            if (string.IsNullOrWhiteSpace(section))
                return prompt;

            return (prompt ?? string.Empty).TrimEnd() + Environment.NewLine + Environment.NewLine + section;
        }

        private class SourceTagMapScope : IDisposable
        {
            private readonly List<string> _previousSourceTagMaps;
            private readonly List<TargetTextNormalizationPlan> _previousTargetTextNormalizationPlans;
            private bool _disposed;

            public SourceTagMapScope(List<string> previousSourceTagMaps, List<TargetTextNormalizationPlan> previousTargetTextNormalizationPlans)
            {
                _previousSourceTagMaps = previousSourceTagMaps;
                _previousTargetTextNormalizationPlans = previousTargetTextNormalizationPlans;
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                _sourceTagMaps.Value = _previousSourceTagMaps;
                _targetTextNormalizationPlans.Value = _previousTargetTextNormalizationPlans;
                _disposed = true;
            }
        }

        private static TargetTextNormalizationPlan GetTargetTextNormalizationPlan()
        {
            var plans = _targetTextNormalizationPlans.Value;
            if (plans == null || plans.Count == 0)
                return null;

            return plans[0];
        }

        public static string NormalizeTargetTextForPrompt(string targetText, TargetTextNormalizationPlan plan)
        {
            if (string.IsNullOrEmpty(targetText) || plan?.Rules == null || plan.Rules.Count == 0)
                return targetText;

            var result = targetText;
            foreach (var rule in plan.Rules)
            {
                if (rule == null || string.IsNullOrEmpty(rule.Token))
                    continue;

                if (rule.IsLineBreak)
                {
                    result = ReplaceFirstAny(result, rule.Token, "\r\n", "\n", "\r", "<br/>", "<br />", "<br>");
                    continue;
                }

                if (rule.Candidates == null || rule.Candidates.Count == 0)
                    continue;

                foreach (var candidate in rule.Candidates)
                {
                    var replaced = ReplaceFirst(result, candidate, rule.Token);
                    if (!string.Equals(replaced, result, StringComparison.Ordinal))
                    {
                        result = replaced;
                        break;
                    }
                }
            }

            return result;
        }

        public static string NormalizeContextTargetTextForPrompt(string sourceText, string targetText)
        {
            if (string.IsNullOrEmpty(sourceText) || string.IsNullOrEmpty(targetText))
                return targetText;

            var sourceTokens = new List<string>();
            var sourceMatches = _sourceTagTokenRegex.Matches(sourceText);
            foreach (Match match in sourceMatches)
            {
                if (match.Success && !string.IsNullOrWhiteSpace(match.Value))
                    sourceTokens.Add(match.Value);
            }

            if (sourceTokens.Count == 0)
                return targetText;

            var markers = new List<(int Index, int Length, string Value)>();
            var markerMatches = _contextTargetMarkerRegex.Matches(targetText);
            foreach (Match match in markerMatches)
            {
                if (!match.Success)
                    continue;

                var value = match.Value;
                if (!ShouldNormalizeContextMarker(value))
                    continue;

                markers.Add((match.Index, match.Length, value));
            }

            if (markers.Count == 0)
                return targetText;

            var replaceCount = Math.Min(sourceTokens.Count, markers.Count);
            if (replaceCount <= 0)
                return targetText;

            var result = new StringBuilder(targetText);
            for (int i = replaceCount - 1; i >= 0; i--)
            {
                var marker = markers[i];
                result.Remove(marker.Index, marker.Length);
                result.Insert(marker.Index, sourceTokens[i]);
            }

            var normalized = result.ToString();
            if (!string.Equals(normalized, targetText, StringComparison.Ordinal))
                LoggingHelper.Verbose($"Preview Helper context target normalized to source-like tag tokens. SourceTokenCount={sourceTokens.Count}, ReplacedMarkerCount={replaceCount}");

            return normalized;
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

        private static bool ShouldNormalizeContextMarker(string marker)
        {
            if (string.IsNullOrEmpty(marker))
                return false;

            if (marker == "\r\n" || marker == "\n" || marker == "\r")
                return true;

            if (_sourceTagTokenRegex.IsMatch(marker))
                return false;

            if (!marker.StartsWith("<", StringComparison.Ordinal) || !marker.EndsWith(">", StringComparison.Ordinal))
                return false;

            var inner = marker.Substring(1, marker.Length - 2).Trim();
            if (inner.Length == 0)
                return false;

            if (inner.StartsWith("/", StringComparison.Ordinal))
                return false;

            int splitIndex = inner.IndexOfAny(new[] { ' ', '/', '\t', '\r', '\n' });
            var tagName = splitIndex >= 0 ? inner.Substring(0, splitIndex) : inner;
            if (_htmlLikeContextTags.Contains(tagName))
                return false;

            return true;
        }


        private static CurrentIndex GetSegmIndex(string prjGuid, string docGuid, string srcLang, string tgtLang)
        {
            var startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            while (true)
            {
                var currentIndex = ContextHelper.Instance.GetCurrentIndex(prjGuid, docGuid, srcLang, tgtLang);

                if (currentIndex.IndexStart != -1 && currentIndex.IndexEnd != -1)
                {
                    return currentIndex;
                }

                if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - startTime > 10000)
                {
                    throw new Exception("Gets the current segment index timeout. Try reactivates the current segment");
                }

                Thread.Sleep(50);
            }
        }

        private static string GetSegmentPreviewProperty(string prjGuid, string docGuid, string srcLang, string tgtLang,
            int startIndex, int endIndex, string propertyName)
        {
            var values = new List<string>();

            for (int i = startIndex; i <= endIndex; i++)
            {
                var value = ContextHelper.Instance.GetPreviewProperty(prjGuid, docGuid, srcLang, tgtLang, i, propertyName);
                if (!string.IsNullOrWhiteSpace(value))
                    values.Add(value.Trim());
            }

            if (values.Count == 0)
                return string.Empty;

            if (string.Equals(propertyName, MemoQ.PreviewInterfaces.Entities.PropertyNames.CharCount, StringComparison.OrdinalIgnoreCase))
            {
                int total = 0;
                bool allNumeric = true;
                foreach (var value in values)
                {
                    if (!int.TryParse(value, out var parsed))
                    {
                        allNumeric = false;
                        break;
                    }

                    total += parsed;
                }

                if (allNumeric)
                    return total.ToString();
            }

            if (string.Equals(propertyName, MemoQ.PreviewInterfaces.Entities.PropertyNames.LineLengthLimit, StringComparison.OrdinalIgnoreCase))
            {
                var distinctValues = new List<string>();
                foreach (var value in values)
                {
                    if (!distinctValues.Contains(value))
                        distinctValues.Add(value);
                }

                if (distinctValues.Count == 1)
                    return distinctValues[0];

                return string.Join("/", distinctValues);
            }

            return values.Count == 1 ? values[0] : string.Join(Environment.NewLine, values);
        }

    }

    class PromptBuilder
    {
        private string _systemPrompt;

        private string _userPrompt;

        private List<Placeholder> _systemPlaceholders;

        private List<Placeholder> _userPlaceholders;

        private HashSet<string> _knownPlaceholderNames;

        private Dictionary<string, string> _placeholderReplacementDic = new Dictionary<string, string>();

        private static readonly Regex _placeholderRegex = new Regex(
            @"(\[(?:(?!{{).)*?\])?(\s*){{([\w\-]+)(!)?}}(\s*)(\[(?:(?!{{).)*?\](?!\s*{{))?",
            RegexOptions.Singleline);


        public PromptBuilder(string systemPrompt, string userPrompt, HashSet<string> knownPlaceholderNames)
        {
            this._systemPrompt = systemPrompt;
            this._userPrompt = userPrompt;

            this._knownPlaceholderNames = knownPlaceholderNames;

            this._systemPlaceholders = GetPlaceholders(systemPrompt, true);
            this._userPlaceholders = GetPlaceholders(userPrompt, false);
        }


        public bool HasPlaceholder(string name)
        {
            foreach (var plhd in _systemPlaceholders)
            {
                if (plhd.Name.Equals(name)) return true;
            }

            foreach (var plhd in _userPlaceholders)
            {
                if (plhd.Name.Equals(name)) return true;
            }

            return false;
        }

        public void SetPlaceholder(string name, string replacement)
        {
            if (string.IsNullOrWhiteSpace(replacement))
            {
                foreach (var plhd in _systemPlaceholders)
                {
                    if (plhd.Name.Equals(name) && plhd.NoWhiteSpace) throw new Exception($"placeholder '{{{name}!}}' has an white space value.");
                }

                foreach (var plhd in _userPlaceholders)
                {
                    if (plhd.Name.Equals(name) && plhd.NoWhiteSpace) throw new Exception($"placeholder '{{{name}!}}' has an white space value.");
                }
            }

            _placeholderReplacementDic[name] = replacement;
        }

        public void ClearPlaceholder(string name)
        {
            _placeholderReplacementDic[name] = string.Empty;
        }

        public (string, string) BuildPrompts()
        {
            return (BuildSystemPrompt(), BuildUserPrompt());
        }

        public string BuildSystemPrompt()
        {
            return Build(_systemPrompt, _systemPlaceholders);
        }

        public string BuildUserPrompt()
        {
            return Build(_userPrompt, _userPlaceholders);
        }


        private List<Placeholder> GetPlaceholders(string prompt, bool isSystem)
        {
            var placeholders = new List<Placeholder>();

            var matches = _placeholderRegex.Matches(prompt);
            for (int i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                var plhd = new Placeholder()
                {
                    Leading = match.Groups[1].Success ? match.Groups[1].Value.Trim('[', ']') : string.Empty,
                    LeadingWhitespace = match.Groups[2].Value,
                    Name = match.Groups[3].Value,
                    NoWhiteSpace = match.Groups[4].Success,
                    TrailingWhitespace = match.Groups[5].Value,
                    Trailing = match.Groups[6].Success ? match.Groups[6].Value.Trim('[', ']') : string.Empty,
                    Position = match.Index,
                    Length = match.Length,
                    IsSystem = isSystem
                };

                if (_knownPlaceholderNames.Contains(plhd.Name)) placeholders.Add(plhd);
            }

            return placeholders;
        }

        private string Build(string prompt, List<Placeholder> placeholders)
        {
            var result = new StringBuilder(prompt);

            for (int i = placeholders.Count - 1; i >= 0; i--)
            {
                string leading = placeholders[i].Leading;
                string leadingWhitespace = placeholders[i].LeadingWhitespace;
                string name = placeholders[i].Name;
                bool noWhiteSpace = placeholders[i].NoWhiteSpace;
                string trailingWhitespace = placeholders[i].TrailingWhitespace;
                string trailing = placeholders[i].Trailing;
                int position = placeholders[i].Position;
                int length = placeholders[i].Length;

                if (_placeholderReplacementDic.TryGetValue(name, out string replacement))
                {
                    if (string.IsNullOrWhiteSpace(replacement))
                    {
                        result.Remove(position, length);
                    }
                    else
                    {
                        result.Remove(position, length);
                        result.Insert(position, leading + leadingWhitespace + replacement + trailingWhitespace + trailing);
                    }
                }
            }

            return result.ToString();
        }


        private class Placeholder
        {
            public string Leading { get; set; }

            public string Name { get; set; }

            public bool NoWhiteSpace { get; set; }

            public string LeadingWhitespace { get; set; }

            public string TrailingWhitespace { get; set; }

            public string Trailing { get; set; }

            public int Position { get; set; }

            public int Length { get; set; }

            public bool IsSystem { get; set; }
        }
    }
}
