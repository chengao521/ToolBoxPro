using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ToolBoxPro.Core.Interfaces;

namespace ToolBoxPro.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private string _welcomeMessage = "欢迎使用 ToolBox Pro";

        [ObservableProperty]
        private string _subtitle = "多功能文件处理工具箱";

        [ObservableProperty]
        private List<ToolInfo> _tools = new()
        {
            new ToolInfo
            {
                Name = "PDF拆分",
                Description = "将PDF文件按页、范围或间隔拆分成多个文件",
                Icon = "📄",
                ViewName = "PdfSplitPage",
                Category = "PDF工具"
            },
            new ToolInfo
            {
                Name = "PDF合并",
                Description = "将多个PDF文件合并为一个文件（即将推出）",
                Icon = "📑",
                ViewName = "",
                Category = "PDF工具",
                IsComingSoon = true
            },
            new ToolInfo
            {
                Name = "图片转换",
                Description = "批量转换图片格式，支持调整尺寸和质量",
                Icon = "🖼️",
                ViewName = "ImageConverterPage",
                Category = "图片工具"
            },
            new ToolInfo
            {
                Name = "批量重命名",
                Description = "批量重命名文件，支持多种命名模式",
                Icon = "📁",
                ViewName = "FileRenamePage",
                Category = "文件工具"
            },
            new ToolInfo
            {
                Name = "重复文件查找",
                Description = "扫描并找出重复的文件（即将推出）",
                Icon = "🔍",
                ViewName = "",
                Category = "文件工具",
                IsComingSoon = true
            },
            new ToolInfo
            {
                Name = "文件压缩",
                Description = "压缩文件和文件夹（即将推出）",
                Icon = "📦",
                ViewName = "",
                Category = "文件工具",
                IsComingSoon = true
            },
            new ToolInfo
            {
                Name = "3D区划地图",
                Description = "根据 GeoJSON 生成可交互 Three.js 3D 地图页面",
                Icon = "🌍",
                ViewName = "ThreeDMapPage",
                Category = "可视化工具"
            }
        };

        public HomeViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        [RelayCommand]
        private async Task NavigateToToolAsync(ToolInfo tool)
        {
            if (tool == null || string.IsNullOrEmpty(tool.ViewName) || tool.IsComingSoon)
                return;

            await _navigationService.NavigateToAsync(tool.ViewName);
        }
    }

    public class ToolInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string ViewName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsComingSoon { get; set; }
    }
}
