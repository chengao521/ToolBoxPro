using ToolBoxPro.Core.Models;

namespace ToolBoxPro.Core.Interfaces
{
    public interface IFileService
    {
        Task<OperationResult<int>> BatchRenameAsync(BatchRenameOptions options);
        Task<OperationResult<List<string>>> FindDuplicatesAsync(string directory, bool includeSubdirectories = false);
        Task<OperationResult<long>> CalculateDirectorySizeAsync(string directory);
    }

    public class BatchRenameOptions
    {
        public string Directory { get; set; } = string.Empty;
        public string SearchPattern { get; set; } = "*.*";
        public string NewNamePattern { get; set; } = "{name}_{index}{ext}";
        public int StartIndex { get; set; } = 1;
        public bool IncludeSubdirectories { get; set; } = false;
        public RenameMode Mode { get; set; } = RenameMode.Sequential;
        public string? FindText { get; set; }
        public string? ReplaceText { get; set; }
    }

    public enum RenameMode
    {
        Sequential,
        FindReplace,
        AddPrefix,
        AddSuffix,
        ChangeExtension
    }
}
