using ToolBoxPro.Core.Models;

namespace ToolBoxPro.Core.Interfaces
{
    public interface IPdfService
    {
        Task<OperationResult<int>> GetPageCountAsync(string filePath);
        Task<OperationResult<List<string>>> SplitPdfAsync(PdfSplitOptions options);
        Task<OperationResult<bool>> ValidatePdfAsync(string filePath);
    }
}
