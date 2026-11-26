using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ToolBoxPro.Core.Interfaces;
using System.Collections.ObjectModel;
using System.IO;

namespace ToolBoxPro.ViewModels
{
    public partial class FileRenameViewModel : ObservableObject
    {
        private readonly IFileService _fileService;
        private readonly IDialogService _dialogService;

        [ObservableProperty]
        private string _sourceDirectory = string.Empty;

        [ObservableProperty]
        private string _searchPattern = "*.*";

        [ObservableProperty]
        private RenameMode _selectedMode = RenameMode.Sequential;

        [ObservableProperty]
        private string _newNamePattern = "{name}_{index}{ext}";

        [ObservableProperty]
        private int _startIndex = 1;

        [ObservableProperty]
        private string? _findText;

        [ObservableProperty]
        private string? _replaceText;

        [ObservableProperty]
        private bool _includeSubdirectories;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private int _fileCount;

        public IEnumerable<RenameMode> RenameModes => Enum.GetValues<RenameMode>();

        public FileRenameViewModel(IFileService fileService, IDialogService dialogService)
        {
            _fileService = fileService;
            _dialogService = dialogService;
        }

        [RelayCommand]
        private void SelectDirectory()
        {
            var folderPath = _dialogService.OpenFolderDialog("选择要处理的文件夹");
            if (!string.IsNullOrEmpty(folderPath))
            {
                SourceDirectory = folderPath;
                UpdateFileCount();
            }
        }

        [RelayCommand]
        private void PreviewFiles()
        {
            if (string.IsNullOrEmpty(SourceDirectory) || !Directory.Exists(SourceDirectory))
            {
                _dialogService.ShowMessage("请先选择有效的文件夹", "提示", MessageType.Warning);
                return;
            }

            UpdateFileCount();
            _dialogService.ShowMessage($"找到 {FileCount} 个文件匹配当前模式", "文件预览", MessageType.Information);
        }

        [RelayCommand]
        private async Task ExecuteRename()
        {
            if (string.IsNullOrEmpty(SourceDirectory) || !Directory.Exists(SourceDirectory))
            {
                _dialogService.ShowMessage("请先选择有效的文件夹", "提示", MessageType.Warning);
                return;
            }

            if (!_dialogService.ShowConfirmation($"确定要重命名 {FileCount} 个文件吗？\n此操作不可撤销！", "确认重命名"))
            {
                return;
            }

            IsBusy = true;
            StatusMessage = "正在重命名文件...";

            try
            {
                var options = new BatchRenameOptions
                {
                    Directory = SourceDirectory,
                    SearchPattern = SearchPattern,
                    NewNamePattern = NewNamePattern,
                    StartIndex = StartIndex,
                    IncludeSubdirectories = IncludeSubdirectories,
                    Mode = SelectedMode,
                    FindText = FindText,
                    ReplaceText = ReplaceText
                };

                var result = await _fileService.BatchRenameAsync(options);

                if (result.Success)
                {
                    StatusMessage = result.Message ?? "重命名完成";
                    _dialogService.ShowMessage($"成功重命名 {result.Data} 个文件！", "完成", MessageType.Information);
                }
                else
                {
                    StatusMessage = result.Message ?? "重命名失败";
                    _dialogService.ShowMessage(result.Message ?? "重命名失败", "错误", MessageType.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"错误: {ex.Message}";
                _dialogService.ShowMessage($"重命名过程中发生错误: {ex.Message}", "错误", MessageType.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void Clear()
        {
            SourceDirectory = string.Empty;
            SearchPattern = "*.*";
            NewNamePattern = "{name}_{index}{ext}";
            StartIndex = 1;
            FindText = null;
            ReplaceText = null;
            IncludeSubdirectories = false;
            FileCount = 0;
            StatusMessage = string.Empty;
        }

        private void UpdateFileCount()
        {
            try
            {
                if (Directory.Exists(SourceDirectory))
                {
                    var searchOption = IncludeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                    var files = Directory.GetFiles(SourceDirectory, SearchPattern, searchOption);
                    FileCount = files.Length;
                }
            }
            catch
            {
                FileCount = 0;
            }
        }
    }
}
