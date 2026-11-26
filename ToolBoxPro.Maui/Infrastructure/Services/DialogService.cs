using System.Collections.ObjectModel;
using Microsoft.Maui.ApplicationModel;
using ToolBoxPro.Core.Interfaces;

namespace ToolBoxPro.Infrastructure.Services
{
    public class DialogService : IDialogService
    {
        public async Task<string?> OpenFileDialogAsync(string filter = "All files (*.*)|*.*", string? title = null)
        {
            try
            {
                var result = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = title ?? "选择文件"
                });
                return result?.FullPath;
            }
            catch
            {
                return null;
            }
        }

        public async Task<IEnumerable<string>?> OpenMultiFileDialogAsync(string filter = "All files (*.*)|*.*", string? title = null)
        {
            try
            {
                var results = await FilePicker.Default.PickMultipleAsync(new PickOptions
                {
                    PickerTitle = title ?? "选择文件"
                });
                return results?.Select(r => r.FullPath);
            }
            catch
            {
                return null;
            }
        }

        public async Task<string?> SaveFileDialogAsync(string filter = "All files (*.*)|*.*", string? title = null, string? defaultFileName = null)
        {
            try
            {
                var folderResult = await PickFolderAsync();
                if (folderResult == null) return null;
                var fileName = defaultFileName ?? "file";
                return Path.Combine(folderResult, fileName);
            }
            catch
            {
                return null;
            }
        }

        public async Task<string?> OpenFolderDialogAsync(string? title = null)
        {
            try
            {
                return await PickFolderAsync();
            }
            catch
            {
                return null;
            }
        }

        public void ShowMessage(string message, string title = "提示", MessageType type = MessageType.Information)
        {
            _ = Application.Current.MainPage?.DisplayAlert(title, message, "确定");
        }

        public async Task<bool> ShowConfirmationAsync(string message, string title = "确认")
        {
            var result = await Application.Current.MainPage.DisplayAlert(title, message, "确定", "取消");
            return result;
        }

        internal static Task<string?> PickFolderAsync()
        {
#if WINDOWS
            try
            {
                var picker = new Windows.Storage.Pickers.FolderPicker();
                picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
                picker.FileTypeFilter.Add("*");
                var folderTask = picker.PickSingleFolderAsync();
                return folderTask.AsTask().ContinueWith(t => t.Result?.Path, TaskContinuationOptions.NotOnFaulted);
            }
            catch
            {
                return Task.FromResult<string?>(null);
            }
#else
            return Task.FromResult<string?>(null);
#endif
        }
    }
}
