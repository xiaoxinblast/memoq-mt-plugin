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

在原始项目基础上，本分支进行了以下改进：

- **修复**：View 视图场景下上下文发送失效问题
- **改进**：上下文标签加入序号（`[上文1 原文]` `[上文1 译文]` 等），更清晰地区分多个上下文来源
- **重构**：改进日志排版可读性

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
