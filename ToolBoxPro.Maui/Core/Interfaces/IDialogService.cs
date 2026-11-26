namespace ToolBoxPro.Core.Interfaces
{
    public interface IDialogService
    {
        Task<string?> OpenFileDialogAsync(string filter = "All files (*.*)|*.*", string? title = null);
        Task<IEnumerable<string>?> OpenMultiFileDialogAsync(string filter = "All files (*.*)|*.*", string? title = null);
        Task<string?> SaveFileDialogAsync(string filter = "All files (*.*)|*.*", string? title = null, string? defaultFileName = null);
        Task<string?> OpenFolderDialogAsync(string? title = null);
        void ShowMessage(string message, string title = "提示", MessageType type = MessageType.Information);
        Task<bool> ShowConfirmationAsync(string message, string title = "确认");
    }

    public enum MessageType
    {
        Information,
        Warning,
        Error
    }
}
