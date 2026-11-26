# PdfToolKit 项目 Code Wiki

## 项目概述

**PdfToolKit** 是一个基于 .NET 8.0 WPF 的 PDF 处理工具箱，采用 MVVM 架构设计，当前提供 PDF 拆分功能，并规划了 PDF 合并和压缩等功能。

- **项目类型**: Windows WPF 桌面应用
- **目标框架**: .NET 8.0-windows
- **架构模式**: MVVM (Model-View-ViewModel)
- **依赖注入**: Microsoft.Extensions.DependencyInjection

---

## 目录结构

```
PdfToolKit/
├── Converters/              # XAML 值转换器
├── Core/
│   ├── Interfaces/          # 核心接口定义
│   └── Models/              # 数据模型
├── Infrastructure/
│   └── Services/            # 基础设施服务实现
├── Services/                # 业务服务
├── ViewModels/              # 视图模型
├── Views/                   # XAML 视图
├── App.xaml                 # 应用程序入口
├── MainWindow.xaml          # 主窗口
└── PdfToolKit.csproj        # 项目配置
```

---

## 核心架构

### 1. 依赖注入配置

应用程序在 [App.xaml.cs](file:///d:/WorkSpace/Tools/PdfToolKit/App.xaml.cs#L28-L41) 中通过 `ConfigureServices` 方法配置所有依赖项：

- **Singleton 服务**: `IPdfService`, `IDialogService`, `INavigationService`
- **Transient 服务**: ViewModels, Views, MainWindow

### 2. MVVM 分层架构

```
┌─────────────────────────────────────────┐
│           Views (UI 层)                  │
│  - MainWindow.xaml                      │
│  - HomeView.xaml                        │
│  - PdfSplitView.xaml                    │
└──────────────┬──────────────────────────┘
               │ 数据绑定
┌──────────────▼──────────────────────────┐
│       ViewModels (逻辑层)               │
│  - MainViewModel                        │
│  - HomeViewModel                        │
│  - PdfSplitViewModel                    │
└──────────────┬──────────────────────────┘
               │ 接口调用
┌──────────────▼──────────────────────────┐
│   Core/Interfaces (抽象层)              │
│  - IPdfService                          │
│  - IDialogService                       │
│  - INavigationService                   │
└──────────────┬──────────────────────────┘
               │ 实现
┌──────────────▼──────────────────────────┐
│  Services & Infrastructure (实现层)    │
│  - PdfProcessingService                 │
│  - DialogService                        │
│  - NavigationService                    │
└─────────────────────────────────────────┘
```

---

## 核心模块详解

### 1. 核心接口 (Core/Interfaces)

#### IPdfService
**文件**: [Core/Interfaces/IPdfService.cs](file:///d:/WorkSpace/Tools/PdfToolKit/Core/Interfaces/IPdfService.cs)

PDF 处理服务的核心接口，定义了所有 PDF 相关操作：

| 方法 | 说明 |
|------|------|
| `GetPageCountAsync(string)` | 获取 PDF 文件总页数 |
| `SplitPdfAsync(PdfSplitOptions)` | 按指定选项拆分 PDF |
| `ValidatePdfAsync(string)` | 验证 PDF 文件有效性 |

#### IDialogService
**文件**: [Core/Interfaces/IDialogService.cs](file:///d:/WorkSpace/Tools/PdfToolKit/Core/Interfaces/IDialogService.cs)

提供对话框和文件选择功能的接口：

| 方法 | 说明 |
|------|------|
| `OpenFileDialog()` | 打开文件选择对话框 |
| `SaveFileDialog()` | 打开文件保存对话框 |
| `OpenFolderDialog()` | 打开文件夹选择对话框 |
| `ShowMessage()` | 显示消息框 |
| `ShowConfirmation()` | 显示确认对话框 |

#### INavigationService
**文件**: [Core/Interfaces/INavigationService.cs](file:///d:/WorkSpace/Tools/PdfToolKit/Core/Interfaces/INavigationService.cs)

页面导航服务接口：

| 方法/属性 | 说明 |
|-----------|------|
| `NavigateTo(string)` | 导航到指定视图 |
| `NavigateTo(string, object)` | 带参数导航 |
| `NavigateBack()` | 导航回上一页 |
| `CanNavigateBack` | 是否可以返回 |
| `Initialize(Frame)` | 初始化导航框架 |
| `RegisterView()` | 注册视图 |

---

### 2. 数据模型 (Core/Models)

#### OperationResult&lt;T&gt;
**文件**: [Core/Models/OperationResult.cs](file:///d:/WorkSpace/Tools/PdfToolKit/Core/Models/OperationResult.cs)

通用操作结果包装类，用于统一处理操作的成功/失败状态：

| 属性 | 类型 | 说明 |
|------|------|------|
| `Success` | bool | 操作是否成功 |
| `Message` | string? | 操作消息 |
| `Data` | T? | 返回数据 |
| `Exception` | Exception? | 异常信息 |

#### PdfSplitOptions & SplitMode
**文件**: [Core/Models/PdfSplitOptions.cs](file:///d:/WorkSpace/Tools/PdfToolKit/Core/Models/PdfSplitOptions.cs)

PDF 拆分配置选项：

| 属性 | 类型 | 说明 |
|------|------|------|
| `SourceFilePath` | string | 源文件路径 |
| `OutputDirectory` | string | 输出目录 |
| `Mode` | SplitMode | 拆分模式 |
| `StartPage` | int? | 起始页 |
| `EndPage` | int? | 结束页 |
| `PageInterval` | int? | 页面间隔 |
| `PageRanges` | List&lt;int[]&gt;? | 页面范围列表 |
| `OutputFileNamePattern` | string? | 输出文件命名模式 |

**SplitMode 枚举**:
- `ByPage`: 每页拆分为单独文件
- `ByRange`: 按指定范围拆分
- `ByInterval`: 按间隔拆分
- `ExtractPages`: 提取指定页面范围

---

### 3. 服务实现

#### PdfProcessingService
**文件**: [Services/PdfProcessingService.cs](file:///d:/WorkSpace/Tools/PdfToolKit/Services/PdfProcessingService.cs)

核心 PDF 处理服务实现，使用 PdfSharpCore 库：

**主要方法**:
| 方法 | 功能 |
|------|------|
| `SplitByPage()` | 每页拆分为单独文件 |
| `SplitByRange()` | 按范围列表拆分 |
| `SplitByInterval()` | 按固定间隔拆分 |
| `ExtractPages()` | 提取指定页面范围 |

#### DialogService
**文件**: [Infrastructure/Services/DialogService.cs](file:///d:/WorkSpace/Tools/PdfToolKit/Infrastructure/Services/DialogService.cs)

使用 Microsoft.Win32 对话框 API 实现的对话框服务。

#### NavigationService
**文件**: [Infrastructure/Services/NavigationService.cs](file:///d:/WorkSpace/Tools/PdfToolKit/Infrastructure/Services/NavigationService.cs)

基于 WPF Frame 的导航服务实现，支持视图注册和导航历史。

---

### 4. ViewModels

#### MainViewModel
**文件**: [ViewModels/MainViewModel.cs](file:///d:/WorkSpace/Tools/PdfToolKit/ViewModels/MainViewModel.cs)

主窗口视图模型，管理导航和侧边栏状态。

| 属性 | 说明 |
|------|------|
| `Title` | 窗口标题 |
| `IsPaneOpen` | 侧边栏是否展开 |

| 命令 | 功能 |
|------|------|
| `TogglePaneCommand` | 切换侧边栏展开状态 |
| `NavigateToCommand` | 导航到指定视图 |

#### PdfSplitViewModel
**文件**: [ViewModels/PdfSplitViewModel.cs](file:///d:/WorkSpace/Tools/PdfToolKit/ViewModels/PdfSplitViewModel.cs)

PDF 拆分工具的视图模型，核心业务逻辑。

| 主要属性 | 说明 |
|----------|------|
| `SourceFilePath` | 源文件路径 |
| `OutputDirectory` | 输出目录 |
| `TotalPages` | PDF 总页数 |
| `SelectedMode` | 选中的拆分模式 |
| `IsBusy` | 是否正在处理 |
| `StatusMessage` | 状态消息 |
| `OutputFiles` | 输出文件列表 |

| 主要命令 | 功能 |
|----------|------|
| `SelectSourceFileCommand` | 选择源 PDF 文件 |
| `SelectOutputDirectoryCommand` | 选择输出目录 |
| `SplitPdfCommand` | 执行拆分操作 |
| `OpenOutputFolderCommand` | 打开输出文件夹 |
| `ClearCommand` | 清空所有输入 |

#### HomeViewModel
**文件**: [ViewModels/HomeViewModel.cs](file:///d:/WorkSpace/Tools/PdfToolKit/ViewModels/HomeViewModel.cs)

首页视图模型，显示可用工具列表。

---

### 5. Views

#### MainWindow
**文件**: [MainWindow.xaml](file:///d:/WorkSpace/Tools/PdfToolKit/MainWindow.xaml)

主窗口，包含可折叠侧边栏和导航框架。

#### PdfSplitView
**文件**: [Views/PdfSplitView.xaml](file:///d:/WorkSpace/Tools/PdfToolKit/Views/PdfSplitView.xaml)

PDF 拆分工具的用户界面，包含文件选择、拆分模式配置和操作按钮。

---

### 6. 转换器 (Converters)

**文件**: [Converters/BooleanToVisibilityConverter.cs](file:///d:/WorkSpace/Tools/PdfToolKit/Converters/BooleanToVisibilityConverter.cs)

提供多种 XAML 值转换器：

| 转换器 | 功能 |
|--------|------|
| `BooleanToVisibilityConverter` | bool 转 Visibility |
| `InverseBooleanConverter` | 反转 bool 值 |
| `IntToVisibilityConverter` | 整数>0 转 Visible |
| `EnumToVisibilityConverter` | 枚举值匹配转 Visible |
| `StringNotEmptyConverter` | 字符串非空转 bool |
| `PaneWidthConverter` | bool 转侧边栏宽度 |

---

## 依赖项

### NuGet 包

| 包名 | 版本 | 用途 |
|------|------|------|
| CommunityToolkit.Mvvm | 8.4.2 | MVVM 框架，提供 ObservableObject 和 RelayCommand |
| Microsoft.Extensions.DependencyInjection | 10.0.8 | 依赖注入容器 |
| PdfSharpCore | 1.3.67 | PDF 处理核心库 |

---

## 运行方式

### 开发环境要求
- .NET 8.0 SDK
- Visual Studio 2022 或更高版本 (推荐)
- Windows 操作系统

### 构建与运行
1. 打开 `PdfToolKit.csproj` 项目
2. 还原 NuGet 包
3. 按 F5 或点击"开始调试"按钮运行

---

## 扩展指南

### 添加新的 PDF 工具

1. **在 Core/Interfaces/IPdfService.cs** 中添加新方法
2. **在 Services/PdfProcessingService.cs** 中实现该方法
3. **创建新的 ViewModel**（继承 ObservableObject）
4. **创建对应的 XAML View**
5. **在 App.xaml.cs** 中注册 View 和 ViewModel
6. **在 MainWindow.xaml.cs** 中注册导航视图
7. **在 HomeViewModel.cs** 中添加工具信息

### 示例：添加 PDF 合并功能

```csharp
// 1. 在 IPdfService 中添加
Task<OperationResult<string>> MergePdfsAsync(List<string> filePaths, string outputPath);

// 2. 在 PdfProcessingService 中实现
public async Task<OperationResult<string>> MergePdfsAsync(...) { ... }

// 3. 创建 PdfMergeViewModel
public partial class PdfMergeViewModel : ObservableObject { ... }

// 4. 创建 PdfMergeView.xaml

// 5-7. 注册并添加到首页
```

---

## 关键流程

### PDF 拆分流程

```
用户选择 PDF 文件
    ↓
PdfSplitViewModel.SelectSourceFileCommand
    ↓
IPdfService.GetPageCountAsync()
    ↓
显示总页数
    ↓
用户配置拆分选项并点击"开始拆分"
    ↓
PdfSplitViewModel.SplitPdfCommand
    ↓
IPdfService.SplitPdfAsync(PdfSplitOptions)
    ↓
根据 SplitMode 调用对应拆分方法
    ↓
返回生成的文件列表
    ↓
更新 UI 显示结果
```

---

## 技术亮点

1. **MVVM 架构**: 清晰的分离关注点，便于测试和维护
2. **依赖注入**: 使用 Microsoft.Extensions.DependencyInjection 实现松耦合
3. **异步操作**: 所有 PDF 操作均为异步，避免 UI 阻塞
4. **统一结果处理**: OperationResult 模式统一处理成功/失败
5. **可扩展设计**: 预留了 PDF 合并、压缩等功能扩展点
6. **响应式 UI**: 使用 CommunityToolkit.Mvvm 的源生成器实现简洁的 INotifyPropertyChanged

---

## 项目状态

- **当前功能**: PDF 拆分（4种模式）
- **即将推出**: PDF 合并、PDF 压缩
