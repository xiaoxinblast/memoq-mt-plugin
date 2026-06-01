# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 项目概述

memoQ 多供应商机器翻译插件（Multi-Supplier-MT-Plugin），为 CAT 工具 memoQ 提供接入国内外数十家 MT/NMT/LLM 翻译服务商的能力。基于 memoQ 官方 MT SDK（`MemoQ.MTInterfaces.dll`）开发，支持传统 NMT 和 OpenAI 兼容的大语言模型。

## 构建与运行

```bash
# 还原 NuGet 包并构建（需 Visual Studio 2022 / MSBuild）
msbuild MT_SDK.sln /p:Configuration=Debug

# 单独构建插件项目
msbuild MultiSupplierMTPlugin/MultiSupplierMTPlugin.csproj /p:Configuration=Debug

# 发布版本（用于分发）
msbuild MultiSupplierMTPlugin/MultiSupplierMTPlugin.csproj /p:Configuration=Release
```

- **目标框架**: .NET Framework 4.8 (`net48`)，Windows Forms 应用
- **解决方案文件**: `MT_SDK.sln`，位于 `source/workspace/Multi-Supplier-MT-Plugin-1.4.1/`
- 构建产物 `MultiSupplierMTPlugin.dll` 放入 memoQ 安装目录 `Addins/` 即可加载
- 四个构建配置: `Debug`, `Release`, `Debug compatible old version`, `Release compatible old version`（后者定义 `COMPATIBLE_OLD_VERSION` 常量以兼容旧版 memoQ）

## 核心架构

### memoQ 插件入口点

memoQ MT 插件通过三个核心类与 memoQ 宿主交互，遵循 memoQ SDK 的 Director → Engine → Session 模式：

| 类 | 基类/接口 | 职责 |
|---|---|---|
| [MultiSupplierMTPluginDirector](source/workspace/Multi-Supplier-MT-Plugin-1.4.1/MultiSupplierMTPlugin/MultiSupplierMTPluginDirector.cs) | `PluginDirectorBase, IModule` | 插件入口，管理配置初始化、选项编辑、Engine 创建 |
| [MultiSupplierMTEngine](source/workspace/Multi-Supplier-MT-Plugin-1.4.1/MultiSupplierMTPlugin/MultiSupplierMTEngine.cs) | `EngineBase` | 翻译引擎，持有限流/重试/服务商实例，创建 Session |
| [MultiSupplierMTSession](source/workspace/Multi-Supplier-MT-Plugin-1.4.1/MultiSupplierMTPlugin/MultiSupplierMTSession.cs) | `ISession, ISessionForStoringTranslations, ISessionWithMetadata` | 单次翻译会话，处理标签、批量、缓存读写 |

`MultiSupplierMTPluginDirector` 通过 DLL 文件名区分多重安装实例，在 `GetOrInitializeOptions()` 中完成全部初始化（日志、缓存、统计、本地化等的初始化串联）。

### 服务商架构

所有翻译服务商实现 [MultiSupplierMTService](source/workspace/Multi-Supplier-MT-Plugin-1.4.1/MultiSupplierMTPlugin/MultiSupplierMTService.cs) 接口。该接口定义三个层级：

```
MultiSupplierMTService (接口)
├── NMTBaseService (接口) → NMTBaseService<TGeneralSettings, TSecureSettings> (抽象类)
│       └── 各 NMT 提供商: Aliyun, Baidu, Tencent, Huoshan, Niutrans, Youdao, Xunfei, Caiyun,
│                          Papago, DeepL, DeepLX, Yandex, 以及 BuiltIn 系列
└── LLMBaseService (接口) → LLMBaseService<TGeneralSettings, TSecureSettings> (抽象类)
        └── OpenAI, Anthropic, 以及所有 OpenAI 兼容提供商（通过通用配置界面）
```

**添加新服务商只需**：在 `Providers/<Name>/` 下新建类，继承 `NMTBaseService<,>` 或 `LLMBaseService<,>`，实现抽象属性（`UniqueName`, `IsAvailable`, `IsBatchSupported`, `SupportLangDic` 等）和 `TranslateAsync` 方法。

`LLMBaseService` 的 `TranslateAsync` 已实现完整的提示词占位符解析流程（术语表、上下文、TM 辅助、批量翻译解析），子类只需实现：
```csharp
protected abstract Task<string> TranslateAsync(TGeneralSettings g, TSecureSettings s, 
    string systemPrompt, string userPrompt, CancellationToken cToken);
```

### 目录结构

```
MultiSupplierMTPlugin/
├── Providers/           # 各服务商实现（一个服务商一个子目录）
│   ├── Aliyun/          # 传统 NMT: 阿里、百度、腾讯、火山、小牛...
│   ├── OpenAI/          # LLM: OpenAI, Anthropic
│   └── ...
├── ProvidersCommon/     # 服务商共享代码
│   ├── Forms/LLM/       # LLM 通用配置界面（批量翻译设置、提示词模板、自定义模型）
│   ├── Options/LLM/     # LLM 通用配置类（LLMBaseGeneralSettings, LLMBaseSecureSettings）
│   ├── Options/NMT/     # NMT 通用配置类
│   └── SupportLanguages/# 语言代码映射表
├── Forms/               # 插件级别窗体（服务商管理、请求限制、缓存、统计日志等）
├── Helpers/             # 工具类（见下文）
├── Languages/           # 多语言资源 (en-US, zh-CN)
├── Localized/           # 本地化辅助类
├── MultiSupplierMTOptions.cs        # 选项模型（XML 序列化持久化）
├── MultiSupplierMTOptionsForm.cs    # 插件主设置界面
├── MultiSupplierMTPluginDirector.cs # 插件入口
├── MultiSupplierMTEngine.cs         # 翻译引擎
├── MultiSupplierMTSession.cs        # 翻译会话
└── MultiSupplierMTService.cs        # 服务商接口定义
```

### Helpers 工具类

| Helper | 职责 |
|---|---|
| `ServiceHelper` | 服务商注册/查找/列表，管理 OpenAI 兼容提供商的自定义添加 |
| `CacheHelper` | 翻译缓存读写，基于 LiteDB 持久化 |
| `LimitHelper` | 令牌桶限流（QPS/并发控制） |
| `RetryHelper` | 失败重试逻辑（超时、重试次数、等待间隔） |
| `BathTranslateHelper` | LLM 批量翻译结果反序列化（JSON/XML 解析） |
| `PromptHelper` | 提示词占位符解析（`{{source-text}}`, `{{glossary-text}}`, `{{tm-target-text}}` 等） |
| `ContextHelper` | 上下文获取，通过 Preview SDK 获取译文、上下文、全文 |
| `GlossaryHelper` | 术语表文件解析（CSV/TXT），智能过滤当前句段术语 |
| `HttpHelper` | HTTP 请求封装（含代理支持） |
| `LoggingHelper` | 文件日志 |
| `StatsHelper` | 请求统计（基于 LiteDB） |
| `OptionsHelper` | 选项初始化与迁移 |
| `DatabaseHelper` | LiteDB 数据库生命周期管理 |
| `SummaryHelper` | 全文摘要生成与缓存 |
| `OthersHelper` | 杂项工具 |

### 关键依赖

**NuGet 包**（在 `.csproj` 中）:
- `Costura.Fody` (6.0.0) — 将依赖 DLL 嵌入主程序集
- `LiteDB` (5.0.21) — 嵌入式 NoSQL 数据库（缓存与统计存储）
- `Newtonsoft.Json` (13.0.3) — JSON 处理
- `NReco.Text.AhoCorasickDoubleArrayTrie` (1.1.1) — 术语表快速多模式匹配（智能术语表功能）

**memoQ SDK DLL**（在 `References/` 目录）:
- `MemoQ.MTInterfaces.dll` — 核心 MT 接口（`EngineBase`, `PluginDirectorBase`, `ISession` 等）
- `MemoQ.Addins.Common.dll` — 插件框架
- `MemoQ.CoreStructures.dll` — memoQ 数据结构
- `MemoQ.PreviewInterfaces.dll` — 预览工具 SDK（用于获取上下文/全文）
- `Kilgray.Utils.dll` — memoQ 通用工具

## 部署与测试

- **部署**: 将 `MultiSupplierMTPlugin.dll` 复制到 `C:\Program Files\memoQ\memoQ-{版本}\Addins\`
- **未签名插件**: 将 `ClientDevConfig.xml` 放入 `%programdata%\MemoQ\` 以跳过每次的加载确认
- **多重安装**: 使用 `DllGenerator.exe` 生成多个 DLL 副本，每个实例可配置不同服务商
- **测试客户端**: `TestClient/` 项目提供独立于 memoQ 的插件测试环境，通过 `PluginHandling/` 加载和调用插件

## memoQ SDK 关键接口说明

- `PluginDirectorBase` — 插件入口，memoQ 通过它发现和加载插件
- `EngineBase` — 翻译引擎，负责创建 Session；`SupportsFuzzyCorrection` 启用 TM 模糊匹配修正
- `ISession` — 单次翻译会话，`TranslateSegments()` 接收句段数组和标签处理回调
- `ISessionWithMetadata` — 接收 `MTRequestMetadata`（ProjectID, Client, Domain 等元数据）
- `ISessionForStoringTranslations` — 接收人工确认的翻译结果（插件用于缓存）
- `IModule` — memoQ 模块生命周期管理

## 配置持久化

`MultiSupplierMTOptions` 通过 memoQ 的 `PluginSettings` 机制序列化。`PluginSettings` 本质是 XML 键值对集合，`MultiSupplierMTOptions` 将其包装为结构化对象，包含 `GeneralSettings`、`SecureSettings` 以及各服务商的 `ProviderOptions`。
