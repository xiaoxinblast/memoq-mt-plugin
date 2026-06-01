# memoQ 多供应商机器翻译插件

此项目基于 [Multi-Supplier-MT-Plugin](https://github.com/JuchiaLu/Multi-Supplier-MT-Plugin) 开源项目进行的修改和改进。

## 项目概述

memoQ 多供应商机器翻译插件（Multi-Supplier-MT-Plugin），为 CAT 工具 [memoQ](https://www.memoq.com/) 提供接入国内外数十家 MT/NMT/LLM 翻译服务商的能力。基于 memoQ 官方 MT SDK 开发，支持传统 NMT 和 OpenAI 兼容的大语言模型。

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

本项目继承了原始项目的开源许可证。

## 致谢

感谢原始项目 [JuchiaLu/Multi-Supplier-MT-Plugin](https://github.com/JuchiaLu/Multi-Supplier-MT-Plugin) 的作者和贡献者。
