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

## 相比上游的改进（v1.5.0）

### 更好的翻译质量

**memoQ 标签不再丢失含义**
带格式标记翻译时，原文中的标签（如加粗、斜体、引用、索引等）不再是空洞的 `<inline_tag id="5"/>`，插件现在会把标签的真实含义（如 `[Bold]`、`[Index: 关键词]`）传给大模型，译文标签放对位置的概率大幅提升。

**LLM 思考模式**
支持 Claude、GPT-5、DeepSeek、Gemini 等模型的思考/推理模式。翻译复杂长句或专业文本时，可以让模型先"想清楚"再动笔。在服务商设置中即可开关和调节思考强度，插件会自动适配不同提供商的 API 参数。

**译文不丢内容**
原版遇到模型输出被截断（finish_reason = length）时只是记一条日志，空译文可能被当作正确结果缓存下来。现在会正确报错并触发重试。

### 更好的上下文

- 发送给大模型的上下文句段现在带有 `[上文1 原文]` `[下文2 译文]` 等标签，模型能清楚地知道每句话的位置和角色，翻译连贯性更好。

### 更容易排查问题

- 每次翻译请求有独立编号，日志一目了然。
- API 请求和响应的 Prompt/Response 不再挤成一团，分行展示、用分隔线隔开，复制出来就能直接看。
- 缓存命中、批次拆分、并发排队、请求失败等关键环节都有日志记录，出问题时不用再猜。

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
