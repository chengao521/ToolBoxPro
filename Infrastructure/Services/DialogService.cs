using Microsoft.Win32;
using ToolBoxPro.Core.Interfaces;
using System.Windows;

namespace ToolBoxPro.Infrastructure.Services
{
    public class DialogService : IDialogService
    {
        public string? OpenFileDialog(string filter = "All files (*.*)|*.*", string? title = null)
        {
            var dialog = new OpenFileDialog
            {
                Filter = filter,
                Title = title ?? "选择文件"
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        public IEnumerable<string>? OpenMultiFileDialog(string filter = "All files (*.*)|*.*", string? title = null)
        {
            var dialog = new OpenFileDialog
            {
                Filter = filter,
                Title = title ?? "选择文件",
                Multiselect = true
            };

            return dialog.ShowDialog() == true ? dialog.FileNames : null;
        }

        public string? SaveFileDialog(string filter = "All files (*.*)|*.*", string? title = null, string? defaultFileName = null)
        {
            var dialog = new SaveFileDialog
            {
                Filter = filter,
                Title = title ?? "保存文件",
                FileName = defaultFileName
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        public string? OpenFolderDialog(string? title = null)
        {
            var dialog = new OpenFolderDialog
            {
                Title = title ?? "选择文件夹"
            };

            return dialog.ShowDialog() == true ? dialog.FolderName : null;
        }

        public void ShowMessage(string message, string title = "提示", MessageType type = MessageType.Information)
        {
            MessageBoxImage icon = type switch
            {
                MessageType.Warning => MessageBoxImage.Warning,
                MessageType.Error => MessageBoxImage.Error,
                _ => MessageBoxImage.Information
            };

            MessageBox.Show(message, title, MessageBoxButton.OK, icon);
        }

        public bool ShowConfirmation(string message, string title = "确认")
        {
            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }
    }
}
