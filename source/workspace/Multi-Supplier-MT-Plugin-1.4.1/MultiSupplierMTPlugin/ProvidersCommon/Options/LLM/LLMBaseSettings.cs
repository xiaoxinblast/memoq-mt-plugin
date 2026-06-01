namespace MultiSupplierMTPlugin.ProvidersCommon.Options.LLM
{
    class LLMBaseGeneralSettings : ProviderGeneralSettings
    {
        public virtual string BaseURL { get; set; } = string.Empty;
        public virtual string Path { get; set; } = "/chat/completions";

        public virtual int MaxTokens { get; set; } = 4096;
        public virtual double Temperature { get; set; } = 1.0;

        public virtual ThinkingMode ThinkingMode { get; set; } = ThinkingMode.ProviderDefault;
        public virtual ThinkingStrength ThinkingStrength { get; set; } = ThinkingStrength.ProviderDefault;

        public virtual string Model { get; set; } = string.Empty;
        public virtual ModelItem[] UserModels { get; set; } = new ModelItem[0];
        public virtual string[] HidenBuildInModels { get; set; } = new string[0];

        public virtual bool PromptCache { get; set; } = false;

        public virtual string PromptTemplateId { get; set; } = "Default";
        public virtual string SystemPrompt { get; set; } = string.Empty;
        public virtual string UserPrompt { get; set; } = string.Empty;
        public virtual string BathTranslateSystemPrompt { get; set; } = string.Empty;
        public virtual string BathTranslateUserPrompt { get; set; } = string.Empty;

        public virtual bool EnableBathTranslate { get; set; } = false;
        public virtual int BathTranslateMaxSegments { get; set; } = 0;
        public virtual int BathTranslateMaxCharacters { get; set; } = 3000;
        public virtual BathTranslateSchema BathTranslateSchema { get; set; } = BathTranslateSchema.Shorter;
        public virtual BathTranslateResponseFormat BathTranslateResponseFormat { get; set; } = BathTranslateResponseFormat.JSON_Object;
    }

    class LLMBaseSecureSettings : ProviderSecureSettings
    {
        public virtual string ApiKey { get; set; } = string.Empty;

        public virtual string Organization { get; set; } = string.Empty;
    }


    enum BathTranslateSchema
    {
        Shorter,
        Longer
    }

    enum ThinkingMode
    {
        ProviderDefault = 0,
        Off = 1,
        On = 2
    }

    enum ThinkingStrength
    {
        ProviderDefault = 0,
        Low = 1,
        Medium = 2,
        High = 3,
        None = 4,
        Minimal = 5,
        XHigh = 6,
        Budget1024 = 7,
        Budget2048 = 8,
        Budget4096 = 9,
        Budget8192 = 10,
        Budget10000 = 11,
        Budget24576 = 12
    }

    enum BathTranslateResponseFormat
    {
        Text,
        JSON_Object,
        JSON_Schema
    }

    class ModelItem
    {
        public string UniqueName { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;
    }

}
