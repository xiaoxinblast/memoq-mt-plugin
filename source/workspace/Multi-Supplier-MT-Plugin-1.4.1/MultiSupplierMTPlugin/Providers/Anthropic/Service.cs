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

namespace MultiSupplierMTPlugin.Providers.Anthropic
{
    class Service : LLMBaseService<GeneralSettings, SecureSettings>
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        static Service()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("anthropic-version", "2023-06-01");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("anthropic-beta", "prompt-caching-2024-07-31");
        }

        public Service(MultiSupplierMTOptions mtOptions, ProviderOptions options) : base(mtOptions, options) { }


        public override string UniqueName { get; set; } = ServiceNames.Anthropic_LLM;

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

        public override string ApiKeyLink { get; set; } = "https://console.anthropic.com/settings/keys";

        public override string ApiDocLink { get; set; } = "https://docs.anthropic.com/en/api/messages";

        public override string ModelsLink { get; set; } = "https://docs.anthropic.com/en/docs/about-claude/models/all-models";

        public override ModelItem[] BuildInModels { get; set; } = new string[]
        {
            "claude-mythos-preview",
            "claude-opus-4-7",
            "claude-opus-4-6",
            "claude-sonnet-4-6",
            "claude-opus-4-5",

            "claude-3-haiku-20240307",
            "claude-3-sonnet-20240229",
            "claude-3-opus-20240229",
            "claude-3-opus-latest",

            "claude-3-5-haiku-20241022",
            "claude-3-5-haiku-latest",

            "claude-3-5-sonnet-20240620",
            "claude-3-5-sonnet-20241022",
            "claude-3-5-sonnet-latest", //推荐

            "claude-3-7-sonnet-20250219",
            "claude-3-7-sonnet-latest" //推荐
        }.Select(name => new ModelItem() { UniqueName = name, DisplayName = name }).ToArray();

        public override Dictionary<string, string> SupportLangDic { get; set; } = LLMSupportLang.Dic;

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

            // 发送请求
            var modelResponse = await _httpClient
               .Get(g.BaseURL + "/models?limit=1000")
               .AddHeader("x-api-key", s.XApiKey)
               .ReceiveJson<ModelResponse>(cToken);

            // 返回最终结果
            return modelResponse.Data.Select(m => m.Id).OrderBy(i => i, new NaturalSortComparer()).ToList();
        }

        private static Thinking BuildThinking(GeneralSettings g, string localizedName)
        {
            var model = NormalizeModel(g.Model);

            if (g.ThinkingMode == ThinkingMode.ProviderDefault)
                return null;

            if (g.ThinkingMode == ThinkingMode.Off)
                return new Thinking() { Type = "disabled" };

            var budgetTokens = ResolveManualThinkingBudget(g, localizedName);
            if (!budgetTokens.HasValue)
                return null;

            return new Thinking()
            {
                Type = "enabled",
                BudgetTokens = budgetTokens.Value
            };
        }

        private static OutputConfig BuildOutputConfig(GeneralSettings g)
        {
            return null;
        }

        private static int? ResolveManualThinkingBudget(GeneralSettings g, string localizedName)
        {
            int desiredBudget;
            switch (g.ThinkingStrength)
            {
                case ThinkingStrength.Budget1024:
                case ThinkingStrength.Low:
                    desiredBudget = 1024;
                    break;
                case ThinkingStrength.Budget2048:
                case ThinkingStrength.ProviderDefault:
                case ThinkingStrength.Medium:
                    desiredBudget = 2048;
                    break;
                case ThinkingStrength.Budget4096:
                case ThinkingStrength.High:
                    desiredBudget = 4096;
                    break;
                case ThinkingStrength.Budget8192:
                    desiredBudget = 8192;
                    break;
                case ThinkingStrength.Budget10000:
                    desiredBudget = 10000;
                    break;
                case ThinkingStrength.Budget24576:
                    desiredBudget = 24576;
                    break;
                default:
                    desiredBudget = 2048;
                    break;
            }

            int maxBudget = g.MaxTokens - 1;
            if (maxBudget < 1024)
            {
                LoggingHelper.Warn($"{localizedName} Thinking budget skipped because max_tokens must be greater than 1024 for manual thinking.");
                return null;
            }

            return Math.Min(desiredBudget, maxBudget);
        }

        private static string ResolveEffort(ThinkingMode thinkingMode, ThinkingStrength thinkingStrength)
        {
            if (thinkingMode == ThinkingMode.Off)
                return null;

            switch (thinkingStrength)
            {
                case ThinkingStrength.Low:
                    return "low";
                case ThinkingStrength.Medium:
                    return "medium";
                case ThinkingStrength.High:
                    return "high";
                default:
                    return null;
            }
        }

        private static bool SupportsAdaptiveThinking(string model)
        {
            var normalized = NormalizeModel(model);
            return normalized.StartsWith("claude-opus-4-7") ||
                   normalized.StartsWith("claude-opus-4-6") ||
                   normalized.StartsWith("claude-sonnet-4-6") ||
                   normalized.StartsWith("claude-mythos-preview");
        }

        private static bool SupportsDisabledThinking(string model)
        {
            return !NormalizeModel(model).StartsWith("claude-mythos-preview");
        }

        private static bool SupportsEffortOutputConfig(string model)
        {
            var normalized = NormalizeModel(model);
            return SupportsAdaptiveThinking(normalized) ||
                   normalized.StartsWith("claude-opus-4-5");
        }

        private static string NormalizeModel(string model)
        {
            return string.IsNullOrWhiteSpace(model)
                ? string.Empty
                : model.Trim().ToLowerInvariant();
        }

        protected override async Task<string> TranslateAsync(GeneralSettings g, SecureSettings s, string systemPrompt, string userPrompt, CancellationToken cToken)
        {
            var localizedName = ServiceLocalizedNameHelper.Get(UniqueName);

            // 6.请求体
            var system = new SystemItem() { Type = "text", Text = systemPrompt };
            if (g.PromptCache)
            {
                system.CacheControl = new CacheControl() { Type = "ephemeral" };
            }

            var anthropicRequest = new AnthropicRequest()
            {
                Model = g.Model,
                MaxTokens = g.MaxTokens,
                Temperature = g.Temperature,
                Thinking = BuildThinking(g, localizedName),
                OutputConfig = BuildOutputConfig(g),
                System = new SystemItem[] { system },
                Messages = new Message[] { new Message() { Role = "user", Content = userPrompt } }
            };

            // 7.请求体批量翻译格式处理
            // Claude 无这一步

            // 8.发送请求
            var anthropicResponse = await _httpClient.Post(g.BaseURL + g.Path)
                .AddHeader("x-api-key", s.XApiKey)
                .SetBodyJson(anthropicRequest)
                .ReceiveJson<AnthropicResponse>(cToken);

            // 9.读取结果中关心的字段
            // Claude 无这一步

            // 10.日志警告非正常响应
            if (!"end_turn".Equals(anthropicResponse?.StopReason, StringComparison.OrdinalIgnoreCase) || !string.IsNullOrEmpty(anthropicResponse?.Error?.Message))
            {
                LoggingHelper.Warn($"{localizedName} Abnormal Finish Reason\r\n" +
                    $"Response did not complete normally — expected finish reason 'end_turn', but got '{anthropicResponse?.StopReason}'.\r\n" +
                    $"{anthropicResponse?.Error?.Message}");
            }

            // 11.读取翻译结果内容字段
            var content = string.Concat((anthropicResponse?.Content ?? new ContentBlock[0])
                .Where(item => "text".Equals(item?.Type, StringComparison.OrdinalIgnoreCase))
                .Select(item => item?.Text ?? string.Empty));

            if (string.IsNullOrEmpty(content))
                throw new HttpRequestException("Anthropic response does not contain a text content block.");

            // 12.日志记录响应结果
            LoggingHelper.Info("-- Response --");
            LoggingHelper.Multiline(content);

            // 13.日志记录 token 使用情况
            LoggingHelper.Info($"Tokens | In={anthropicResponse?.Usage?.InputTokens}" +
                $"(cacheW={anthropicResponse?.Usage?.CacheCreationInputTokens},cacheR={anthropicResponse?.Usage?.CacheReadInputTokens})" +
                $" | Out={anthropicResponse?.Usage?.OutputTokens}");

            return content;
        }
    }
}
