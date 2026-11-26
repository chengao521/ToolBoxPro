using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.IO;
using ToolBoxPro.Core.Interfaces;

namespace ToolBoxPro.ViewModels
{
    public partial class ThreeDMapViewModel : ObservableObject
    {
        private readonly IThreeDMapService _mapService;
        private readonly IDialogService _dialogService;

        [ObservableProperty]
        private string _title = "3D 中国地图";

        [ObservableProperty]
        private string _geoJsonUrl = "https://geo.datav.aliyun.com/areas_v3/bound/100000_full.json";

        [ObservableProperty]
        private string _outputPath = string.Empty;

        [ObservableProperty]
        private string _projectionCenterLongitude = "104";

        [ObservableProperty]
        private string _projectionCenterLatitude = "37.5";

        [ObservableProperty]
        private int _extrudeDepth = 10;

        [ObservableProperty]
        private string _topColor = "#0a2a3a";

        [ObservableProperty]
        private string _sideColor = "#0d3b4f";

        [ObservableProperty]
        private string _borderColor = "#ffffff";

        [ObservableProperty]
        private string _highlightColor = "#ffeb3b";

        [ObservableProperty]
        private bool _showStars = true;

        [ObservableProperty]
        private bool _showLabels;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _statusMessage = "准备就绪";

        public ThreeDMapViewModel(IThreeDMapService mapService, IDialogService dialogService)
        {
            _mapService = mapService;
            _dialogService = dialogService;
        }

        [RelayCommand]
        private void SelectOutputPath()
        {
            var path = _dialogService.SaveFileDialog("HTML 文件 (*.html)|*.html|所有文件 (*.*)|*.*", "保存 3D 地图页面", "3d-map.html");
            if (!string.IsNullOrWhiteSpace(path))
            {
                OutputPath = path;
            }
        }

        [RelayCommand]
        private async Task GenerateMap()
        {
            if (string.IsNullOrWhiteSpace(GeoJsonUrl))
            {
                _dialogService.ShowMessage("请输入 GeoJSON 数据地址", "提示", MessageType.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(OutputPath))
            {
                SelectOutputPath();
                if (string.IsNullOrWhiteSpace(OutputPath)) return;
            }

            IsBusy = true;
            StatusMessage = "正在生成 3D 地图页面...";

            try
            {
                var options = CreateOptions();
                var result = await _mapService.GenerateMapAsync(options);

                if (result.Success && !string.IsNullOrWhiteSpace(result.Data))
                {
                    StatusMessage = result.Message ?? "生成成功";
                    _dialogService.ShowMessage($"3D 地图页面已生成：\n{result.Data}", "生成成功", MessageType.Information);
                }
                else
                {
                    StatusMessage = result.Message ?? "生成失败";
                    _dialogService.ShowMessage(StatusMessage, "错误", MessageType.Error);
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task GenerateAndOpen()
        {
            await GenerateMap();
            if (!string.IsNullOrWhiteSpace(OutputPath) && File.Exists(OutputPath))
            {
                Process.Start(new ProcessStartInfo(OutputPath) { UseShellExecute = true });
            }
        }

        [RelayCommand]
        private void OpenOutputFolder()
        {
            if (string.IsNullOrWhiteSpace(OutputPath) || !File.Exists(OutputPath)) return;
            Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{OutputPath}\"") { UseShellExecute = true });
        }

        [RelayCommand]
        private void UseChinaPreset()
        {
            Title = "3D 中国地图";
            GeoJsonUrl = "https://geo.datav.aliyun.com/areas_v3/bound/100000_full.json";
            ProjectionCenterLongitude = "104";
            ProjectionCenterLatitude = "37.5";
        }

        [RelayCommand]
        private void UseZhejiangPreset()
        {
            Title = "3D 浙江省地图";
            GeoJsonUrl = "https://geo.datav.aliyun.com/areas_v3/bound/330000_full.json";
            ProjectionCenterLongitude = "120.2";
            ProjectionCenterLatitude = "29.2";
        }

        [RelayCommand]
        private void ResetStyle()
        {
            ExtrudeDepth = 10;
            TopColor = "#0a2a3a";
            SideColor = "#0d3b4f";
            BorderColor = "#ffffff";
            HighlightColor = "#ffeb3b";
            ShowStars = true;
            ShowLabels = false;
        }

        private ThreeDMapOptions CreateOptions()
        {
            return new ThreeDMapOptions
            {
                Title = Title,
                GeoJsonUrl = GeoJsonUrl,
                OutputPath = OutputPath,
                ProjectionCenterLongitude = ProjectionCenterLongitude,
                ProjectionCenterLatitude = ProjectionCenterLatitude,
                ExtrudeDepth = ExtrudeDepth,
                TopColor = TopColor,
                SideColor = SideColor,
                BorderColor = BorderColor,
                HighlightColor = HighlightColor,
                ShowStars = ShowStars,
                ShowLabels = ShowLabels
            };
        }
    }
}
