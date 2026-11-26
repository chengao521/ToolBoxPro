using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ToolBoxPro.Core.Interfaces;
using System.Collections.ObjectModel;
using System.IO;

namespace ToolBoxPro.ViewModels
{
    public partial class ImageConverterViewModel : ObservableObject
    {
        private readonly IImageService _imageService;
        private readonly IDialogService _dialogService;

        [ObservableProperty]
        private ObservableCollection<string> _sourceFiles = new();

        [ObservableProperty]
        private string _outputDirectory = string.Empty;

        [ObservableProperty]
        private ImageFormat _selectedFormat = ImageFormat.Jpeg;

        [ObservableProperty]
        private int _quality = 90;

        [ObservableProperty]
        private int? _maxWidth;

        [ObservableProperty]
        private int? _maxHeight;

        [ObservableProperty]
        private bool _keepAspectRatio = true;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private ObservableCollection<string> _outputFiles = new();

        public IEnumerable<ImageFormat> ImageFormats => Enum.GetValues<ImageFormat>();

        public ImageConverterViewModel(IImageService imageService, IDialogService dialogService)
        {
            _imageService = imageService;
            _dialogService = dialogService;
        }

        [RelayCommand]
        private void SelectSourceFiles()
        {
            var filePaths = _dialogService.OpenMultiFileDialog(
                "图片文件|*.jpg;*.jpeg;*.png;*.bmp;*.gif|所有文件|*.*",
                "选择图片文件");

            if (filePaths != null && filePaths.Any())
            {
                foreach (var file in filePaths)
                {
                    if (!SourceFiles.Contains(file))
                    {
                        SourceFiles.Add(file);
                    }
                }
                StatusMessage = $"已选择 {SourceFiles.Count} 个文件";
            }
        }

        [RelayCommand]
        private void RemoveFile(string file)
        {
            if (SourceFiles.Contains(file))
            {
                SourceFiles.Remove(file);
                StatusMessage = $"已选择 {SourceFiles.Count} 个文件";
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

        [RelayCommand]
        private async Task ConvertImages()
        {
            if (SourceFiles.Count == 0)
            {
                _dialogService.ShowMessage("请先选择图片文件", "提示", MessageType.Warning);
                return;
            }

            if (string.IsNullOrEmpty(OutputDirectory))
            {
                _dialogService.ShowMessage("请选择输出文件夹", "提示", MessageType.Warning);
                return;
            }

            IsBusy = true;
            StatusMessage = "正在转换图片...";

            try
            {
                var options = new ImageConvertOptions
                {
                    SourceFiles = SourceFiles.ToList(),
                    OutputDirectory = OutputDirectory,
                    TargetFormat = SelectedFormat,
                    Quality = Quality,
                    MaxWidth = MaxWidth,
                    MaxHeight = MaxHeight,
                    KeepAspectRatio = KeepAspectRatio
                };

                var result = await _imageService.ConvertImagesAsync(options);

                if (result.Success)
                {
                    OutputFiles.Clear();
                    foreach (var file in result.Data ?? new List<string>())
                    {
                        OutputFiles.Add(file);
                    }

                    StatusMessage = result.Message ?? "转换完成";
                    _dialogService.ShowMessage($"成功转换图片！\n共生成 {OutputFiles.Count} 个文件", "完成", MessageType.Information);
                }
                else
                {
                    StatusMessage = result.Message ?? "转换失败";
                    _dialogService.ShowMessage(result.Message ?? "转换失败", "错误", MessageType.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"错误: {ex.Message}";
                _dialogService.ShowMessage($"转换过程中发生错误: {ex.Message}", "错误", MessageType.Error);
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
            SourceFiles.Clear();
            OutputDirectory = string.Empty;
            Quality = 90;
            MaxWidth = null;
            MaxHeight = null;
            KeepAspectRatio = true;
            OutputFiles.Clear();
            StatusMessage = string.Empty;
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
    }
}
