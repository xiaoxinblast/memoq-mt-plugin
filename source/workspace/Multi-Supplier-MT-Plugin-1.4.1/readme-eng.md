[中文](https://github.com/JuchiaLu/Multi-Supplier-MT-Plugin/blob/main/readme.md) [English](https://github.com/JuchiaLu/Multi-Supplier-MT-Plugin/blob/main/readme-eng.md)

## 1. Why This Plugin Was Created

Recently, I needed to translate some technical documents. To ensure consistency of specific professional terms, I explored CAT (Computer-Assisted Translation) software and ultimately chose memoQ. ~~It has a PDF external preview plugin that allows real-time preview of the segment's position in the original PDF, which was a key reason for my choice~~ (After using it for a while, I found it didn't fully meet my needs, so I developed a [Word External Preview Plugin for memoQ](https://github.com/JuchiaLu/Memoq-Word-Preview)).

Since memoQ is foreign software, it has almost no integration with domestic translation service providers. While the existing Tmxmall and Intento plugins integrate dozens of providers on their backend, they don't allow users to input the providers' own API keys. Many providers offer monthly free quotas, which cannot be utilized through Tmxmall or Intento. Furthermore, neither of these plugins has a translation caching feature. Hence, this plugin was created.

The plugin has only been tested on memoQ versions 9.14 and 11.4. If issues arise in other versions, please provide feedback. Given memoQ's steep learning curve, a [tutorial with best practices](https://github.com/JuchiaLu/Multi-Supplier-MT-Plugin/blob/main/doc/BestPractice.md) has been written.

## 2. Translation Plugin Features Preview

![](https://raw.githubusercontent.com/JuchiaLu/Multi-Supplier-MT-Plugin/master/images/preview.png)

---

✔ Multiple Service Providers (hundreds), Multiple Installations (install the plugin multiple times), Multi-language Interface

✔ Provider Management: Add custom OpenAI compatible providers, Enable or disable providers

✔ Large Language Model Support: Batch translation, Glossary, Context, Translation Memory, Full-text Summary

---

✔ Request Type: Plain Text, Include Formatting, Include Formatting and Tags

✔ Formatting or Tag Representation: Xml, Html

✔ Requests Without Tags: Append tags from source to target text (optional)

✔ Requests With Tags: Normalize whitespace around tags in target text (optional)

---

✔ Batch Translation: Multiple segments require only one request

✔ Parallel Requests: Multiple requests can be sent simultaneously

✔ Request Size Limit: A single request can contain a limited number of segments or characters

✔ Request Rate Limit: Maximum number of requests allowed within a certain time period

✔ Request Concurrency Limit: Maximum number of requests executing concurrently at any time

✔ Request Retry on Failure: Timeout duration, Retry wait time, Maximum retry attempts

---

✔ Translation Cache: Enabled by default, stored in a database, permanent validity

✔ Other Features: Request logs, Request count statistics, Custom display name

---

<details>
<summary>Expand to View</summary>
<pre>
Store (Human) Translation Results: (Director.StoringTranslationSupported), (When confirming a translation in memoQ, the source and target text are sent to the plugin, which can be used for self-learning or stored as a translation cache), (Supported, currently only used for caching)<br/>
Use "MT (Machine Translation)" to Correct "TM (Translation Memory)": (Engine.SupportsFuzzyCorrection), (If a source segment has a TM match that isn't perfect, memoQ will attempt to improve the suggestion by sending the differences to MT for translation), (Supported, not currently used)<br/>
Use "MetaData" to Assist "MT (Machine Translation)": (ISessionWithMetadata interface), (Metadata from the user's project settings, e.g., ProjectID, Client, Domain, Subject, etc., can be used to assist translation, available starting from memoQ 9.14), (Supported, used to assist in obtaining target text, above/below context, summary, full text)<br/>
Use "TM (Translation Memory)" to Assist "MT (Machine Translation)": (Director.SupportFuzzyForwarding), (In addition to the source segment to be translated, memoQ also sends the source and target text of the best available TM match to the MT, available starting from memoQ 10.0), (Supported, users can utilize it in LLM prompts)<br/>
</pre>
</details>

---

## 3. How to Install the Translation Plugin

![](https://raw.githubusercontent.com/JuchiaLu/Multi-Supplier-MT-Plugin/master/images/installed.png)

Place `MultiSupplierMTPlugin.dll` from the Release into the memoQ plugin directory:

- For memoQ: `C:\Program Files\memoQ\memoQ-{version}\Addins`.
- For memoQ Server: `C:\Program Files\Kilgray\MemoQ Server\Addins`.

The plugin is unsigned. memoQ will ask whether to load it upon each startup. To avoid this prompt:

- For memoQ: Place `ClientDevConfig.xml` into the `%programdata%\MemoQ` directory.
- For memoQ Server: Place `UserApprovedUnsignedMTplugins.xml` into the `%programdata%\MemoQ Server` directory.

## 4. How to Perform Multiple Installations

![](https://raw.githubusercontent.com/JuchiaLu/Multi-Supplier-MT-Plugin/master/images/multi%20install%202.png)

Use `Dll Generator.exe` from the Release to generate the required number of DLL files. Place these, along with `MultiSupplierMTPlugin.dll`, into the plugin directory.

While you can rename the DLLs arbitrarily, it's not recommended because when the next version is released, you would have to manually rename them back to their original names to associate with saved configurations.

Always use `Dll Generator.exe` to generate DLLs instead of simply copying and renaming them, otherwise multiple installations will cause errors, unless [this](https://github.com/dotnet/aspnetcore/issues/47465) issue is resolved.

## 5. Explanation of Tag Request Types

![](https://raw.githubusercontent.com/JuchiaLu/Multi-Supplier-MT-Plugin/master/images/formattings%20and%20tags.png)

In the source text, "Hello" is bold, and "World!" is red. The differences in source text requests for machine translation are as follows:

- Text Only: `Hello World!`
- Include Formatting: `<b>Hello<\b> World!`
- Include Formatting and Tags: `<b>Hello<\b> <inline_tag id="0">World!<inline_tag id="0"/>`

To maintain the same layout effect in the translation as the original, it's required to place formatting (like bold, italic, underline, superscript, subscript, etc.) and tags (like font color, background color, links, etc.) in appropriate positions in the translation. Formatting or tags can be represented in two forms: using the Xml standard or using the Html standard. Check the "Supports Xml or Html" column in the provider table for each provider's support status.

## 6. Custom Request Limit Settings

- If the service provider supports batch translation, the plugin defaults to limiting each request to a maximum of 3000 characters, with no limit on the number of segments per request.
- If the service provider does not support batch translation, the plugin forces each request to translate only 1 segment, with no limit on the number of characters per request.
- If QPS is known, the plugin's default rate limit is set to 90% of the known QPS, and the default concurrency limit is set equal to the known QPS value.
- If QPS is unknown, the plugin defaults the rate limit to 5 requests per second, and the default concurrency limit is also set to a maximum of 5 requests executing simultaneously.
- The default timeout duration, retry wait time, and maximum retry attempts for all providers are 0, meaning no retry is performed by default.

Users can customize request limits. When size, rate, or concurrency limits are set to 0, it means no limit. When the retry limit is set to 0, it means no retry. During pre-translation (batch translation), the maximum number of segments is also limited by memoQ, defaulting to a maximum of 10 segments. This can be changed by modifying the `BatchSize` value in `C:\Program Files\memoQ\memoQ-{version}\MemoQ.exe.config`.

## 7. Large Language Model Prompt Caching

Mainstream LLM providers now support [Prompt Caching](https://api-docs.deepseek.com/zh-cn/news/news0802), which reduces service latency and cost.

When using caching, providers have different requirements for input Token length, and the pricing and expiration times for cache writes/reads also differ:

| Provider                                                     | Minimum Input Token Length | Write Cost Multiplier | Read Cost Multiplier | Cache Expiration |
| ------------------------------------------------------------ | -------------------------- | --------------------- | -------------------- | ---------------- |
| [DeepSeek](https://api-docs.deepseek.com/zh-cn/guides/kv_cache) | 64 (same for all models)   | 1.0                   | 0.1                  | Hours to days    |
| [OpenAI](https://platform.openai.com/docs/guides/prompt-caching) | 1024 (same for all models) | 1.0                   | 0.5                  | 5~10 minutes     |
| [Google](https://ai.google.dev/gemini-api/docs/caching#implicit-caching) | 1024 (varies by model)     | 1.0                   | 0.25                 | 3-5 minutes      |
| [Anthropic](https://docs.anthropic.com/en/docs/build-with-claude/prompt-caching) | 1024 (varies by model)     | 1.25                  | 0.1                  | 5 minutes        |

Taking Anthropic Claude as an example, assuming the prompt length is fixed at 10,000 Tokens each time:

- First request: performs a cache write operation, consuming 1 * 1.25 = 12.5k input Tokens.
- Subsequent requests: perform cache read operations, consuming 1 * 0.1 = 1k input Tokens.
- If no cache read operation occurs for 5 consecutive minutes, the next request consumes the same as the first request.

During each request, only the **invariant prefix** of the prompt (the characters from the start of the concatenated system and user prompts up to the first character that changes) can be cached.

Therefore, placeholders whose values do not change dynamically across requests (e.g., `{{summary-text}}`, `{{full-text}}`, etc.) should be positioned earlier in the prompt.

Placeholders whose values change dynamically with each request (e.g., `{{glossary-text}}`, `{{source-text}}`, etc.) should be placed later in the prompt.

For providers like DeepSeek and OpenAI, they do not charge extra for prompt cache writes (cost multiplier 1.0), so prompt caching is enabled by default and cannot be turned off.

For Anthropic, writes incur a 25% extra charge (cost multiplier 1.25). If the prompt is not structured as described above, enabling caching might lead to higher costs.

You can enable plugin logging to view Token usage (including prompt cache write and read information) and adjust your prompts promptly to prevent prompt cache invalidation.

## 8. Large Language Model Prompt Placeholders

The following placeholders (available via right-click menu for quick insertion) can be used in "System Prompt" or "User Prompt" and will be replaced by actual content:

---

- `{{source-language}}`: Source language
- `{{target-language}}`: Target language

Restrictions: None

---

- `{{source-text}}`: Source text (text to be translated)
- `{{target-text}}`: Target text (translated text)

Restrictions (applies only to target text): Requires memoQ 9.14+, requires enabling the preview helper, cannot be used in pre-translation.

---

- `{{tm-source-text}}`: Source text of the best matching translation memory entry for the text to be translated
- `{{tm-target-text}}`: Target text of the best matching translation memory entry for the text to be translated

Restrictions: Requires memoQ 10.0+, requires enabling the "Send best fuzzy TM" option.

---

- `{{above-text}}`: Context above the text to be translated (can include translations)
- `{{below-text}}`: Context below the text to be translated (can include translations)

Restrictions: Requires memoQ 9.14+, requires enabling the preview helper, cannot be used in pre-translation.

---

- `{{summary-text}}`: Full-text summary (excludes translations)
- `{{full-text}}`: Full text (excludes translations)

Restrictions: Requires memoQ 9.14+, requires enabling the preview helper.

---

- `{{glossary-text}}`: Glossary

Restrictions: Requires the glossary file encoding to be UTF-8.

---

### 8.1 {{source-language}}, {{target-language}}

(No special notes, omitted)

### 8.2 {{source-text}}, {{target-text}}

The obtained target-text does not contain tag information. Therefore, you cannot use only target-text in the prompt to ask the AI to polish the translation, as the result would lack the original tags.

One workaround is to include both the tagged source-text and the untagged target-text in the prompt, asking the AI to polish the translation while placing the tags from the source-text into appropriate positions in the target text.

Another workaround is to create a clean translation memory, confirm all current translations to save them into the TM, and then use the tm-target-text placeholder instead of the target-text placeholder.

### 8.3 {{tm-source-text}}, {{tm-target-text}}

Translation Memory refers to the translation memory within memoQ, not the plugin's translation cache. These placeholders require memoQ version 10.0 or higher, and the following option must be enabled in the memoQ machine translation settings:

![](https://raw.githubusercontent.com/JuchiaLu/Multi-Supplier-MT-Plugin/master/images/SendBestFuzzyTM.png)

### 8.4 {{above-text}}, {{below-text}}

For regular users, prioritize adjusting memoQ's segmentation rules to produce longer segments, which inherently provides context. This avoids the need to carry context, saving Tokens.

For translation professionals who must use shorter segmentation rules, adjust the number of contextual segments or characters carried based on the actual situation to balance translation quality and Token consumption.

When customizing the `max segments` and `max characters` options for these placeholders: Setting to 0 means no limit. However, to prevent accidentally including the entire text, setting both to 0 simultaneously will result in empty content.

### 8.5 {{summary-text}}, {{full-text}}

The actual content of `{{summary-text}}` can be manually specified, applying to all documents, or automatically generated by an LLM, giving each document its own summary.

The summary is automatically generated only once and then saved to a file cache. It is read from the cache thereafter unless the document is deleted from memoQ and re-imported, or the cache file is manually deleted.

Cache files are saved in the `%appdata%/MemoQ/Plugins/MultiSupplierMTPlugin/Cache/Summary` directory, named as `[summary]-[Original Document Filename]-[Document GUID].txt`, e.g., `[summary]-[My Document.doc]-[1ee7154a-47b7-4e82-bc48-f99b3728f233].txt`. You can inspect and correct the auto-generated summary content.

Use the `{{full-text}}` placeholder cautiously, typically only in prompts for auto-generating summaries. Using it in system or user prompts for translation will include the full text in the request, consuming a significant number of Tokens.

### 8.6 {{glossary-text}}

Store terms in a CSV or TXT file with UTF-8 encoding. The default column names and order are:

```csv
SourceTerm, TargetTerm, SourceLanguage, TargetLanguage
Hello     , 你好       , eng           , zho-CN
World     , 世界       , eng           , zho-CN
```

The default delimiter is a "comma". Language codes use the [3-letter codes](https://docs.memoq.com/current/en/Concepts/concepts-supported-languages.html) specified by memoQ.

The terminology file parsing algorithm is flexible enough. If the structure differs from the default, it should parse correctly as long as the following conventions are met:

- Header: If the column order matches the default order, the first header row is optional. Otherwise, it must contain a header row specifying the correct column order.
- Columns: Must include at least `SourceTerm` and `TargetTerm` columns. Optionally can include `SourceLanguage` and `TargetLanguage` columns.
- Conflict: If a header is present and does not list the language columns, but the data rows contain language columns, the language columns in the data rows will be ignored.
- Spacing: Adding spaces for delimiter alignment is not necessary. Spacing in the example is for aesthetics only and does not affect parsing.

The purpose of the language columns is to restrict the retrieval of term entries. For example, if a terminology file contains term pairs for multiple languages, without the language columns, the final result would include all language pairs.

**Plugin v1.4.1 implements an "Intelligent Glossary" feature. Requests now only carry terms contained within the current segment, unlike before which carried all terms, allowing your glossary to be very large.**

If the terminology file fails to read or any row data parsing fails, the plugin will abort the translation to prevent unexpected results. You can check the error information in the logs and correct the errors.

Note: memoQ automatically sets the input method to full-width mode in Chinese environments. If you need to change the delimiter and the terminology file uses a non-full-width delimiter, first manually switch back to half-width mode.

### 8.7 Special Syntax for Placeholders

To abort the translation request if the placeholder's actual content is empty, add `!` after the placeholder name, e.g., `{{summary-text!}}`, `{{full-text!}}`.

To replace the guiding text associated with a placeholder with empty content when the placeholder's content is empty, add `[]` before or after the placeholder and write the guiding text inside the `[]`.

### 8.8 Placeholder and Special Syntax Example

User Prompt:

````
This is the full-text summary for translation reference:
Summary Start
{{summary-text!}}
Summary End

[This is the glossary for translation reference:
Glossary Start
]{{glossary-text}}[
Glossary End]

[This is the context above the text to be translated for reference:
Above Context Start
]{{above-text}}[
Above Context End]

[This is the context below the text to be translated for reference:
Below Context Start
]{{below-text}}[
Below Context End]

[This is the source and target text from the translation memory for reference:
Translation Memory Start
]{{tm-source-text}}
{{tm-target-text}}[
Translation Memory End]

Please translate the following text from {{source-language!}} to {{target-language!}}:
{{source-text!}}
````

We want to use prompt caching, so we prioritize placing `{{summary-text!}}`, whose value doesn't change dynamically across requests, at the beginning of the prompt.

We want the translation request to abort if `{{summary-text!}}`, `{{source-text!}}`, etc., are empty, so we added `!` to indicate that empty content is forbidden.

We want the guiding text associated with `{{glossary-text}}`, `{{above-text}}`, `{{tm-source-text}}`, etc., to be replaced with empty content if their actual content is empty, so we use the `[]` syntax. For example, if `{{glossary-text}}` is empty, the preceding guiding text `[This is the glossary for translation reference: Glossary Start]` and the following guiding text `[Glossary End]` will also be replaced with empty content.

### 8.9 Why Enable the Preview Helper

The official "Machine Translation SDK" does not provide the ability to obtain target text, context, or full text. When clicking a segment in the memoQ interface, only the source text of that single segment can be obtained. When using pre-translation for batch translation, by default, only the source text of 10 segments can be obtained per request.

This plugin uses the official "Preview Tool SDK" to indirectly obtain target text, context, and full text. Upon first use, a pop-up will request permission to enable the `Multi Supplier MT Plugin Helper` (a virtual preview tool).

![](https://raw.githubusercontent.com/JuchiaLu/Multi-Supplier-MT-Plugin/master/images/EnableHelper.png)

Because the plugin needs to know a segment's index to obtain its target text and context, unfortunately, even using the "Preview Tool SDK," the index can only be obtained when a segment is clicked in the memoQ interface. During batch translation via pre-translation, there is no interface click interaction, so the index of the segment being translated cannot be obtained, and thus its target text and context cannot be retrieved.

## 9. Supported Traditional Translation Providers

| Provider                                                     | Free Quota                                                  | QPS Limit               | Batch Support | Xml or Html Support |
| ------------------------------------------------------------ | ----------------------------------------------------------- | ----------------------- | ------------- | ------------------- |
| [Alibaba](https://help.aliyun.com/zh/machine-translation/developer-reference/api-alimt-2018-10-12-translategeneral) | Standard: 1M chars/month<br />Professional: 1M chars/month | Standard: 50<br/>Professional: 50 | ✔             | ✔ (Html)            |
| [Tencent](https://cloud.tencent.com/document/product/551/40566) | 5M chars/month                                              | 5                       | ✔             | ×                   |
| [Baidu](https://fanyi-api.baidu.com/product/113)             | Standard: 50K chars/month<br/>Advanced: 1M chars/month      | Standard: 1<br/>Advanced: 10 | ?             | ×                   |
| [Volcano](https://www.volcengine.com/docs/4640/65067)        | 2M chars/month                                              | 10                      | ✔             | ×                   |
| [Niutrans](https://niutrans.com/documents/contents/transapi_batch_v2) | 200K chars/day                                              | 5                       | ×             | ✔ (Xml)             |
| [Youdao](https://fanyi.youdao.com/openapi/)                  | New users get 50 RMB credit (permanent)                     | 50                      | ✔             | ×                   |
| [iFlytek](https://www.xfyun.cn/doc/nlp/xftrans/API.html)     | New users get 2M chars (valid for 90 days)                  | ?                       | ×             | ✔ (Xml)             |
| [Caiyun](https://open.caiyunapp.com/%E4%BA%94%E5%88%86%E9%92%9F%E5%AD%A6%E4%BC%9A%E5%BD%A9%E4%BA%91%E5%B0%8F%E8%AF%91_API) | New users get 1M chars (valid for 30 days)                  | 10                      | ✔             | ×                   |
|                                                              |                                                             |                         |               |                     |
| [Papago](https://guide.ncloud-docs.com/docs/en/papagotranslation-api) | 10K chars/day                                               | ?                       | ×             | ×                   |
| [DeepL](https://developers.deepl.com/docs/api-reference/translate/openapi-spec-for-text-translation) | 500K chars/month                                            | ?                       | ✔             | ✔ (Xml or Html)     |
| [DeepLX](https://deeplx.owo.network/endpoints/free.html)     | Self-hosted or developer provides 500K chars/day            | ?                       | ×             | ✔ (Xml or Html)     |
| [Yandex](https://yandex.cloud/en/docs/translate/api-ref/Translation/translate) | ?                                                           | 20                      | ✔             | ✔ (Html)            |
|                                                              |                                                             |                         |               |                     |
| Microsoft (Built-in)                                         | Use wisely while you can                | ?                       | ✔             | ×                   |
| Google (Built-in)                                            | Use wisely while you can                | ?                       | ×             | ✔ (Xml or Html)     |
| DeepL (Built-in)                                             | Use wisely while you can                | ?                       | ×             | ✔ (Xml or Html)     |
| Yandex (Built-in)                                            | Use wisely while you can                | ?                       | ×             | ✔ (Xml or Html)     |
| Lingvanex (Built-in)                                         | Use wisely while you can                | ?                       | ×             | ✔ (Xml or Html)     |
| Modernmt (Built-in)                                          | Use wisely while you can                | ?                       | ×             | ✔ (Xml or Html)     |

Note: Baidu Translate claims to support batch translation, but it uses line breaks to distinguish independent segments. However, a segment provided by memoQ may contain line breaks, so it is treated as not supporting batch translation.

## 10. Supported Large Language Model Providers

| Provider                                                     | Free Quota | QPS Limit | Batch Support | Xml or Html Support |
| ------------------------------------------------------------ | ---------- | --------- | ------------- | ------------------- |
| [Alibaba Bailian](https://help.aliyun.com/zh/model-studio/compatibility-of-openai-with-dashscope) | ?          | ?         | ✔             | ✔(Xml or Html)      |
| [Tencent Hunyuan](https://cloud.tencent.com/document/product/1729/111007) | ?          | ?         | ✔             | ✔(Xml or Html)      |
| [Baidu Qianfan](https://cloud.baidu.com/doc/qianfan-docs/s/1m9l6eex1) | ?          | ?         | ✔             | ✔(Xml or Html)      |
| [ByteDance Volcano](https://www.volcengine.com/docs/82379/1330626) | ?          | ?         | ✔             | ✔(Xml or Html)      |
| [iFlytek Spark](https://www.xfyun.cn/doc/spark/HTTP%E8%B0%83%E7%94%A8%E6%96%87%E6%A1%A3.html#_7-%E4%BD%BF%E7%94%A8openai-sdk%E8%AF%B7%E6%B1%82%E7%A4%BA%E4%BE%8B) | ?          | ?         | ✔             | ✔(Xml or Html)      |
| [DeepSeek](https://api-docs.deepseek.com/zh-cn/)             | ?          | ?         | ✔             | ✔(Xml or Html)      |
| [Zhipu Qingyan](https://bigmodel.cn/dev/api/thirdparty-frame/openai-sdk) | ?          | ?         | ✔             | ✔(Xml or Html)      |
| [StepFun](https://platform.stepfun.com/docs/guide/openai)    | ?          | ?         | ✔             | ✔(Xml or Html)      |
| [Moonshot](https://platform.moonshot.cn/docs/guide/migrating-from-openai-to-kimi) | ?          | ?         | ✔             | ✔(Xml or Html)      |
| [Baichuan AI](https://platform.baichuan-ai.com/docs/api#python-client) | ?          | ?         | ✔             | ✔(Xml or Html)      |
| [MiniMax](https://platform.minimaxi.com/document/ChatCompletion) | ?          | ?         | ✔             | ✔(Xml or Html)      |
| [SenseTime](https://www.sensecore.cn/help/docs/model-as-a-service/nova/overview/compatible-mode) | ?          | ?         | ✔             | ✔(Xml or Html)      |
| [Lingyi Wanwu](https://platform.lingyiwanwu.com/docs)        | ?          | ?         | ✔             | ✔(Xml or Html)      |
| [InternAI](https://internlm.intern-ai.org.cn/doc/docs/Chat/) | ?          | ?         | ✔             | ✔(Xml or Html)      |
| [InflyAI](https://platform.infly.cn/docs/open-api/api)       | ?          | ?         | ✔             | ✔(Xml or Html)      |
| [InfiniAI](https://docs.infini-ai.com/gen-studio/api/maas.html) | ?          | ?         | ✔             | ✔(Xml or Html)      |
| [Silicon Flow](https://docs.siliconflow.cn/cn/userguide/quickstart#4-3-openai) | ?          | ?         | ✔             | ✔(Xml or Html)      |
| [360 Zhinao](https://ai.360.com/open)                        | ?          | ?         | ✔             | ✔(Xml or Html)      |
|                                                              |            |           |               |                     |
| [OpenAI](https://platform.openai.com/docs/api-reference/chat) | ?          | ?         | ✔             | ✔(Xml or Html)      |
| [Anthropic](https://docs.anthropic.com/en/api/openai-sdk)    | ?          | ?         | ✔             | ✔(Xml or Html)      |
| [Google](https://ai.google.dev/gemini-api/docs/openai)       | ?          | ?         | ✔             | ✔(Xml or Html)      |
| [xAI](https://docs.x.ai/docs/api-reference#chat-completions) | ?          | ?         | ✔             | ✔(Xml or Html)      |
| [Mistral](https://docs.mistral.ai/api/)                      | ?          | ?         | ✔             | ✔(Xml or Html)      |
| [Cohere](https://docs.cohere.com/docs/compatibility-api)     | ?          | ?         | ✔             | ✔(Xml or Html)      |
| [Upstage](https://console.upstage.ai/docs/getting-started)   | ?          | ?         | ✔             | ✔(Xml or Html)      |
| [Azure](https://learn.microsoft.com/en-us/azure/ai-services/openai/reference) | ?          | ?         | ✔             | ✔(Xml or Html)      |
| [Cloudflare](https://developers.cloudflare.com/workers-ai/configuration/open-ai-compatibility/) | ?          | ?         | ✔             | ✔(Xml or Html)      |
| [Nvidia](https://docs.api.nvidia.com/nim/reference/nvidia-llama-3_1-nemotron-ultra-253b-v1-infer) | ?          | ?         | ✔             | ✔(Xml or Html)      |
| [Open Router](https://openrouter.ai/docs/quickstart)         | ?          | ?         | ✔             | ✔(Xml or Html)      |
| [One API](https://github.com/songquanpeng/one-api)           | ?          | ?         | ✔             | ✔(Xml or Html)      |
| [New API](https://docs.newapi.pro/api/openai-chat/)          | ?          | ?         | ✔             | ✔(Xml or Html)      |
| [Lite LLM](https://docs.litellm.ai/docs/)                    | ?          | ?         | ✔             | ✔(Xml or Html)      |
| [Ollama](https://github.com/ollama/ollama/blob/main/docs/openai.md) | ?          | ?         | ✔             | ✔(Xml or Html)      |
| [vLLM](https://docs.vllm.ai/en/latest/serving/openai_compatible_server.html) | ?          | ?         | ✔             | ✔(Xml or Html)      |
| [LM Studio](https://lmstudio.ai/docs/app/api/endpoints/openai) | ?          | ?         | ✔             | ✔(Xml or Html)      |

Note: The list above includes only some commonly used service providers. The plugin supports nearly all mainstream LLM providers, including self-developed vendors, online aggregation gateways, self-built aggregation gateways, and local large models, totaling nearly a hundred providers.