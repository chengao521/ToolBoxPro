using ToolBoxPro.Core.Interfaces;
using ToolBoxPro.Core.Models;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using System.IO;

namespace ToolBoxPro.Services
{
    public class PdfProcessingService : IPdfService
    {
        public async Task<OperationResult<int>> GetPageCountAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return OperationResult<int>.Fail("文件不存在");
                }

                return await Task.Run(() =>
                {
                    using var document = PdfReader.Open(filePath, PdfDocumentOpenMode.Import);
                    return OperationResult<int>.Ok(document.PageCount);
                });
            }
            catch (Exception ex)
            {
                return OperationResult<int>.Fail($"获取页数失败: {ex.Message}", ex);
            }
        }

        public async Task<OperationResult<List<string>>> SplitPdfAsync(PdfSplitOptions options)
        {
            try
            {
                if (!File.Exists(options.SourceFilePath))
                {
                    return OperationResult<List<string>>.Fail("源文件不存在");
                }

                if (!Directory.Exists(options.OutputDirectory))
                {
                    Directory.CreateDirectory(options.OutputDirectory);
                }

                return await Task.Run(() =>
                {
                    var outputFiles = new List<string>();

                    using var inputDocument = PdfReader.Open(options.SourceFilePath, PdfDocumentOpenMode.Import);
                    var pageCount = inputDocument.PageCount;

                    switch (options.Mode)
                    {
                        case SplitMode.ByPage:
                            outputFiles = SplitByPage(inputDocument, options, pageCount);
                            break;
                        case SplitMode.ByRange:
                            outputFiles = SplitByRange(inputDocument, options, pageCount);
                            break;
                        case SplitMode.ByInterval:
                            outputFiles = SplitByInterval(inputDocument, options, pageCount);
                            break;
                        case SplitMode.ExtractPages:
                            outputFiles = ExtractPages(inputDocument, options, pageCount);
                            break;
                    }

                    return OperationResult<List<string>>.Ok(outputFiles, $"成功拆分为 {outputFiles.Count} 个文件");
                });
            }
            catch (Exception ex)
            {
                return OperationResult<List<string>>.Fail($"拆分PDF失败: {ex.Message}", ex);
            }
        }

        public async Task<OperationResult<bool>> ValidatePdfAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return OperationResult<bool>.Fail("文件不存在");
                }

                return await Task.Run(() =>
                {
                    try
                    {
                        using var document = PdfReader.Open(filePath, PdfDocumentOpenMode.Import);
                        return OperationResult<bool>.Ok(true, "PDF文件有效");
                    }
                    catch
                    {
                        return OperationResult<bool>.Ok(false, "PDF文件无效或已损坏");
                    }
                });
            }
            catch (Exception ex)
            {
                return OperationResult<bool>.Fail($"验证PDF失败: {ex.Message}", ex);
            }
        }

        private List<string> SplitByPage(PdfDocument inputDocument, PdfSplitOptions options, int pageCount)
        {
            var outputFiles = new List<string>();
            var pattern = options.OutputFileNamePattern ?? "page_{0}.pdf";

            for (int i = 0; i < pageCount; i++)
            {
                var outputDocument = new PdfDocument();
                outputDocument.AddPage(inputDocument.Pages[i]);

                var outputPath = Path.Combine(options.OutputDirectory, string.Format(pattern, i + 1));
                outputDocument.Save(outputPath);
                outputFiles.Add(outputPath);
            }

            return outputFiles;
        }

        private List<string> SplitByRange(PdfDocument inputDocument, PdfSplitOptions options, int pageCount)
        {
            var outputFiles = new List<string>();

            if (options.PageRanges == null || options.PageRanges.Count == 0)
            {
                return outputFiles;
            }

            for (int rangeIndex = 0; rangeIndex < options.PageRanges.Count; rangeIndex++)
            {
                var range = options.PageRanges[rangeIndex];
                if (range.Length != 2) continue;

                int startPage = Math.Max(0, range[0] - 1);
                int endPage = Math.Min(pageCount - 1, range[1] - 1);

                if (startPage > endPage) continue;

                var outputDocument = new PdfDocument();
                for (int i = startPage; i <= endPage; i++)
                {
                    outputDocument.AddPage(inputDocument.Pages[i]);
                }

                var outputPath = Path.Combine(options.OutputDirectory, $"range_{rangeIndex + 1}.pdf");
                outputDocument.Save(outputPath);
                outputFiles.Add(outputPath);
            }

            return outputFiles;
        }

        private List<string> SplitByInterval(PdfDocument inputDocument, PdfSplitOptions options, int pageCount)
        {
            var outputFiles = new List<string>();
            int interval = options.PageInterval ?? 1;

            if (interval <= 0) interval = 1;

            int fileIndex = 1;
            for (int i = 0; i < pageCount; i += interval)
            {
                var outputDocument = new PdfDocument();
                int endPage = Math.Min(i + interval, pageCount);

                for (int j = i; j < endPage; j++)
                {
                    outputDocument.AddPage(inputDocument.Pages[j]);
                }

                var outputPath = Path.Combine(options.OutputDirectory, $"part_{fileIndex}.pdf");
                outputDocument.Save(outputPath);
                outputFiles.Add(outputPath);
                fileIndex++;
            }

            return outputFiles;
        }

        private List<string> ExtractPages(PdfDocument inputDocument, PdfSplitOptions options, int pageCount)
        {
            var outputFiles = new List<string>();

            int startPage = Math.Max(0, (options.StartPage ?? 1) - 1);
            int endPage = Math.Min(pageCount - 1, (options.EndPage ?? pageCount) - 1);

            if (startPage > endPage)
            {
                return outputFiles;
            }

            var outputDocument = new PdfDocument();
            for (int i = startPage; i <= endPage; i++)
            {
                outputDocument.AddPage(inputDocument.Pages[i]);
            }

            var outputPath = Path.Combine(options.OutputDirectory, "extracted.pdf");
            outputDocument.Save(outputPath);
            outputFiles.Add(outputPath);

            return outputFiles;
        }
    }
}
