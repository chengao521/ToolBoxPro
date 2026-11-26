namespace ToolBoxPro.Core.Interfaces
{
    public interface IDialogService
    {
        string? OpenFileDialog(string filter = "All files (*.*)|*.*", string? title = null);
        IEnumerable<string>? OpenMultiFileDialog(string filter = "All files (*.*)|*.*", string? title = null);
        string? SaveFileDialog(string filter = "All files (*.*)|*.*", string? title = null, string? defaultFileName = null);
        string? OpenFolderDialog(string? title = null);
        void ShowMessage(string message, string title = "提示", MessageType type = MessageType.Information);
        bool ShowConfirmation(string message, string title = "确认");
    }

    public enum MessageType
    {
        Information,
        Warning,
        Error
    }
}
