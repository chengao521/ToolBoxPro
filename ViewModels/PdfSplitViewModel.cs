using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ToolBoxPro.Core.Interfaces;
using ToolBoxPro.Core.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;

namespace ToolBoxPro.ViewModels
{
    public partial class PdfSplitViewModel : ObservableObject
    {
        private readonly IPdfService _pdfService;
        private readonly IDialogService _dialogService;

        [ObservableProperty]
        private string _sourceFilePath = string.Empty;

        [ObservableProperty]
        private string _outputDirectory = string.Empty;

        [ObservableProperty]
        private int _totalPages;

        [ObservableProperty]
        private SplitMode _selectedMode = SplitMode.ByPage;

        [ObservableProperty]
        private int? _startPage;

        [ObservableProperty]
        private int? _endPage;

        [ObservableProperty]
        private int? _pageInterval = 1;

        [ObservableProperty]
        private string _pageRangesText = string.Empty;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private ObservableCollection<string> _outputFiles = new();

        public IEnumerable<SplitMode> SplitModes => Enum.GetValues<SplitMode>();

        public PdfSplitViewModel(IPdfService pdfService, IDialogService dialogService)
        {
            _pdfService = pdfService;
            _dialogService = dialogService;
        }

        [RelayCommand]
        private async Task SelectSourceFile()
        {
            var filePath = _dialogService.OpenFileDialog(
                "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*",
                "选择PDF文件");

            if (!string.IsNullOrEmpty(filePath))
            {
                SourceFilePath = filePath;

                var result = await _pdfService.GetPageCountAsync(filePath);
                if (result.Success)
                {
                    TotalPages = result.Data;
                    StatusMessage = $"文件已加载，共 {TotalPages} 页";
                }
                else
                {
                    _dialogService.ShowMessage(result.Message ?? "无法读取文件", "错误", MessageType.Error);
                }
            }
        }

        [RelayCommand]
        private void SelectOutputDirectory()
        {
            var folderPath = _dialogService.OpenFolderDialog("选择输出文件夹");
            if (!string.IsNullOrEmpty(folderPath))
            {
                if (IsDirectoryWritable(folderPath))
                {
                    OutputDirectory = folderPath;
                }
                else
                {
                    _dialogService.ShowMessage("所选目录没有写入权限，请选择其他目录（如桌面或文档）", "权限不足", MessageType.Warning);
                }
            }
        }

        private bool IsDirectoryWritable(string path)
        {
            try
            {
                string testFile = Path.Combine(path, $"_{Guid.NewGuid():N}.tmp");
                using (FileStream fs = File.Create(testFile))
                {
                    fs.WriteByte(0);
                }
                File.Delete(testFile);
                return true;
            }
            catch
            {
                return false;
            }
        }

        [RelayCommand]
        private async Task SplitPdf()
        {
            if (string.IsNullOrEmpty(SourceFilePath))
            {
                _dialogService.ShowMessage("请先选择PDF文件", "提示", MessageType.Warning);
                return;
            }

            if (string.IsNullOrEmpty(OutputDirectory))
            {
                _dialogService.ShowMessage("请选择输出文件夹", "提示", MessageType.Warning);
                return;
            }

            IsBusy = true;
            StatusMessage = "正在拆分PDF...";

            try
            {
                var options = new PdfSplitOptions
                {
                    SourceFilePath = SourceFilePath,
                    OutputDirectory = OutputDirectory,
                    Mode = SelectedMode,
                    StartPage = StartPage,
                    EndPage = EndPage,
                    PageInterval = PageInterval
                };

                if (SelectedMode == SplitMode.ByRange && !string.IsNullOrEmpty(PageRangesText))
                {
                    options.PageRanges = ParsePageRanges(PageRangesText);
                }

                var result = await _pdfService.SplitPdfAsync(options);

                if (result.Success)
                {
                    OutputFiles.Clear();
                    foreach (var file in result.Data ?? new List<string>())
                    {
                        OutputFiles.Add(file);
                    }

                    StatusMessage = result.Message ?? "拆分完成";
                    _dialogService.ShowMessage($"成功拆分PDF！\n共生成 {OutputFiles.Count} 个文件", "完成", MessageType.Information);
                }
                else
                {
                    StatusMessage = result.Message ?? "拆分失败";
                    _dialogService.ShowMessage(result.Message ?? "拆分失败", "错误", MessageType.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"错误: {ex.Message}";
                _dialogService.ShowMessage($"拆分过程中发生错误: {ex.Message}", "错误", MessageType.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void OpenOutputFolder()
        {
            if (!string.IsNullOrEmpty(OutputDirectory) && Directory.Exists(OutputDirectory))
            {
                System.Diagnostics.Process.Start("explorer.exe", OutputDirectory);
            }
        }

        [RelayCommand]
        private void Clear()
        {
            SourceFilePath = string.Empty;
            OutputDirectory = string.Empty;
            TotalPages = 0;
            StartPage = null;
            EndPage = null;
            PageInterval = 1;
            PageRangesText = string.Empty;
            OutputFiles.Clear();
            StatusMessage = string.Empty;
        }

        private List<int[]> ParsePageRanges(string input)
        {
            var ranges = new List<int[]>();
            var parts = input.Split(',', StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                var range = part.Split('-', StringSplitOptions.RemoveEmptyEntries);
                if (range.Length == 2 &&
                    int.TryParse(range[0].Trim(), out int start) &&
                    int.TryParse(range[1].Trim(), out int end))
                {
                    ranges.Add(new[] { start, end });
                }
            }

            return ranges;
        }
    }
}
