using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.IO;
using ToolBoxPro.Core.Interfaces;

namespace ToolBoxPro.ViewModels
{
    public partial class ProteinViewerViewModel : ObservableObject
    {
        private readonly IProteinViewerService _proteinService;
        private readonly IDialogService _dialogService;

        [ObservableProperty]
        private string _title = "蛋白质 3D 结构查看器";

        [ObservableProperty]
        private string _pdbFileUrl = "https://raw.githubusercontent.com/martin-groenhout/atomic-data/master/1TQN.pdb";

        [ObservableProperty]
        private string _outputPath = string.Empty;

        [ObservableProperty]
        private bool _showBallAndStick = true;

        [ObservableProperty]
        private bool _showRibbon = true;

        [ObservableProperty]
        private bool _showLabels;

        [ObservableProperty]
        private string _helixColor = "#FF00FF";

        [ObservableProperty]
        private string _sheetColor = "#FFD700";

        [ObservableProperty]
        private string _coilColor = "#808080";

        [ObservableProperty]
        private string _backgroundColor = "#0a0e1a";

        [ObservableProperty]
        private int _atomRadiusScale = 10;

        [ObservableProperty]
        private int _ribbonWidth = 5;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _statusMessage = "准备就绪";

        public ProteinViewerViewModel(IProteinViewerService proteinService, IDialogService dialogService)
        {
            _proteinService = proteinService;
            _dialogService = dialogService;
        }

        [RelayCommand]
        private void SelectPdbFile()
        {
            var path = _dialogService.OpenFileDialog("PDB 文件 (*.pdb)|*.pdb|所有文件 (*.*)|*.*", "选择 PDB 文件");
            if (!string.IsNullOrWhiteSpace(path))
            {
                PdbFileUrl = path;
            }
        }

        [RelayCommand]
        private void SelectOutputPath()
        {
            var path = _dialogService.SaveFileDialog("HTML 文件 (*.html)|*.html|所有文件 (*.*)|*.*", "保存蛋白质 3D 查看器", "protein-viewer.html");
            if (!string.IsNullOrWhiteSpace(path))
            {
                OutputPath = path;
            }
        }

        [RelayCommand]
        private async Task GenerateViewer()
        {
            if (string.IsNullOrWhiteSpace(PdbFileUrl))
            {
                _dialogService.ShowMessage("请输入 PDB 文件 URL 或选择本地 PDB 文件", "提示", MessageType.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(OutputPath))
            {
                SelectOutputPath();
                if (string.IsNullOrWhiteSpace(OutputPath)) return;
            }

            IsBusy = true;
            StatusMessage = "正在生成蛋白质 3D 查看器...";

            try
            {
                var options = CreateOptions();
                var result = await _proteinService.GenerateViewerAsync(options);

                if (result.Success && !string.IsNullOrWhiteSpace(result.Data))
                {
                    StatusMessage = result.Message ?? "生成成功";
                    _dialogService.ShowMessage($"蛋白质 3D 查看器已生成：\n{result.Data}", "生成成功", MessageType.Information);
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
            await GenerateViewer();
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
        private void UseCYP450Preset()
        {
            Title = "细胞色素 P450 3A4 (1TQN)";
            PdbFileUrl = "https://raw.githubusercontent.com/martin-groenhout/atomic-data/master/1TQN.pdb";
        }

        [RelayCommand]
        private void UseHemoglobinPreset()
        {
            Title = "血红蛋白 (1HHB)";
            PdbFileUrl = "https://raw.githubusercontent.com/martin-groenhout/atomic-data/master/1HHB.pdb";
        }

        [RelayCommand]
        private void UseLysozymePreset()
        {
            Title = "溶菌酶 (1LYZ)";
            PdbFileUrl = "https://raw.githubusercontent.com/martin-groenhout/atomic-data/master/1LYZ.pdb";
        }

        [RelayCommand]
        private void ResetStyle()
        {
            HelixColor = "#FF00FF";
            SheetColor = "#FFD700";
            CoilColor = "#808080";
            BackgroundColor = "#0a0e1a";
            AtomRadiusScale = 10;
            RibbonWidth = 5;
            ShowBallAndStick = true;
            ShowRibbon = true;
            ShowLabels = false;
        }

        private ProteinViewerOptions CreateOptions()
        {
            return new ProteinViewerOptions
            {
                Title = Title,
                PdbFileUrl = PdbFileUrl,
                OutputPath = OutputPath,
                ShowBallAndStick = ShowBallAndStick,
                ShowRibbon = ShowRibbon,
                ShowLabels = ShowLabels,
                HelixColor = HelixColor,
                SheetColor = SheetColor,
                CoilColor = CoilColor,
                BackgroundColor = BackgroundColor,
                AtomRadiusScale = AtomRadiusScale,
                RibbonWidth = RibbonWidth
            };
        }
    }
}
