# memoQ 多供应商机器翻译插件

> **声明**：此项目是 [JuchiaLu/Multi-Supplier-MT-Plugin](https://github.com/JuchiaLu/Multi-Supplier-MT-Plugin) 的 fork，在原作者 [JuchiaLu](https://github.com/JuchiaLu) 的工作基础上进行修改和改进。原始项目采用 [MIT License](https://github.com/JuchiaLu/Multi-Supplier-MT-Plugin/blob/main/LICENSE)。

## 上游项目信息

| 项目 | 详情 |
|---|---|
| **原作者** | [JuchiaLu](https://github.com/JuchiaLu) |
| **原始仓库** | [JuchiaLu/Multi-Supplier-MT-Plugin](https://github.com/JuchiaLu/Multi-Supplier-MT-Plugin) |
| **上游版本** | v1.4.1 |
| **许可证** | [MIT License](source/workspace/Multi-Supplier-MT-Plugin-1.4.1/LICENSE) |
| **Star** | ⭐ 22+ |

## 本 Fork 改动

基于上游 [v1.4.1](https://github.com/JuchiaLu/Multi-Supplier-MT-Plugin) 对照源代码，本分支做了以下改动：

### 日志系统重构（[LoggingHelper.cs](source/workspace/Multi-Supplier-MT-Plugin-1.4.1/MultiSupplierMTPlugin/Helpers/LoggingHelper.cs)）

- 引入请求上下文 ID（`#001`、`#002` …），每次翻译请求关联所有日志行，定位问题更高效
- 新增 `Separator()` 分隔线和 `Multiline()` 结构化多行输出，Prompt/Response 不再混在单行 `\r\n` 里
- 日志格式改为 `HH:mm:ss.fff [TAG] [#ID] message`，比原版 `yyyy-MM-dd HH:mm:ss.fff [LEVEL] - message` 更紧凑

### 上下文标签加入序号（[ContextHelper.cs](source/workspace/Multi-Supplier-MT-Plugin-1.4.1/MultiSupplierMTPlugin/Helpers/ContextHelper.cs)）

- 上下文句段加入 `[上文1 原文]` `[上文1 译文]` `[下文2 原文]` 等序号标签，LLM 更易区分多个上下文来源的对应关系

### memoQ 标签智能处理（[MultiSupplierMTSession.cs](source/workspace/Multi-Supplier-MT-Plugin-1.4.1/MultiSupplierMTPlugin/MultiSupplierMTSession.cs)）

- 新增源标签映射构建（`BuildSourceTagMap`），将 memoQ 内联标签的 `displaytext`/`val` 含义传给 LLM，避免标签语义丢失
- 新增译文标签归一化（`NormalizeDisplayedTagsToSourceTokens`），后处理时自动将显示文本还原为标准标签令牌
- 缓存键加入标签映射，避免不同标签结构的句段命中错误缓存

### LLM 思考模式支持

- **Anthropic**（[Service.cs](source/workspace/Multi-Supplier-MT-Plugin-1.4.1/MultiSupplierMTPlugin/Providers/Anthropic/Service.cs)）：新增 Claude 4.x 系列 thinking/enabled/disabled/budget 配置，支持 `claude-opus-4-7`、`claude-sonnet-4-6` 等新模型，响应内容改为拼接所有 text block
- **OpenAI 及兼容提供商**（[Service.cs](source/workspace/Multi-Supplier-MT-Plugin-1.4.1/MultiSupplierMTPlugin/Providers/OpenAI/Service.cs)）：新增多提供商 thinking 分派 —— OpenAI reasoning_effort、DeepSeek thinking、Google reasoning_effort、阿里云 enable_thinking，自动根据 base URL 和模型名选择合适的 API 参数

### 其他改进

- **语言代码表**：LLM 提供商的语言映射从 `LanguageHelper.CodeToFriendlyNameDic` 切换为 `LLMSupportLang.Dic`
- **异常处理**：LLM 非正常 `finish_reason`（如 `length`、`content_filter`）现在抛出异常而非仅记录日志，避免空译文进入缓存
- **日志覆盖**：关键路径（缓存命中/未命中、批次拆分、并发等待、请求分发等）补充了 Verbose 级别日志

## 项目概述

为 CAT 工具 [memoQ](https://www.memoq.com/) 提供接入国内外数十家 MT/NMT/LLM 翻译服务商的能力。基于 memoQ 官方 MT SDK 开发，支持传统 NMT 和 OpenAI 兼容的大语言模型。

## 主要功能

- **多服务提供商**（上百家）、多重安装、多语言界面
- **大语言模型支持**：批量翻译、术语表、上下文、翻译记忆、全文摘要
- **服务提供商管理**：自定义添加 OpenAI 兼容提供商、启用或禁用提供商
- **翻译缓存**：基于 LiteDB 持久化，默认开启
- **限流与重试**：QPS 限制、并发控制、失败重试
- **请求统计与日志**

## 构建

```bash
# 还原 NuGet 包并构建（需 Visual Studio 2022 / MSBuild）
msbuild MT_SDK.sln /p:Configuration=Debug

# 单独构建插件项目
msbuild MultiSupplierMTPlugin/MultiSupplierMTPlugin.csproj /p:Configuration=Debug

# 发布版本
msbuild MultiSupplierMTPlugin/MultiSupplierMTPlugin.csproj /p:Configuration=Release
```

- **目标框架**: .NET Framework 4.8
- **解决方案文件**: `source/workspace/Multi-Supplier-MT-Plugin-1.4.1/MT_SDK.sln`

## 安装

将构建产物 `MultiSupplierMTPlugin.dll` 复制到 memoQ 插件目录：

- memoQ: `C:\Program Files\memoq\memoq-{版本号}\Addins`
- memoQ Server: `C:\Program Files\Kilgray\MemoQ Server\Addins`

## 许可证

本项目继承上游 [MIT License](source/workspace/Multi-Supplier-MT-Plugin-1.4.1/LICENSE)。

> Copyright (c) 2023 JuchiaLu

## 致谢

感谢原作者 [JuchiaLu](https://github.com/JuchiaLu) 开发和维护 [Multi-Supplier-MT-Plugin](https://github.com/JuchiaLu/Multi-Supplier-MT-Plugin)，为 memoQ 用户提供了一个功能丰富的多供应商机器翻译解决方案。
