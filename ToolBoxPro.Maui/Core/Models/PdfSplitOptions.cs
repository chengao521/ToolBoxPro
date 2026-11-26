namespace ToolBoxPro.Core.Models
{
    public class PdfSplitOptions
    {
        public string SourceFilePath { get; set; } = string.Empty;
        public string OutputDirectory { get; set; } = string.Empty;
        public SplitMode Mode { get; set; } = SplitMode.ByPage;
        public int? StartPage { get; set; }
        public int? EndPage { get; set; }
        public int? PageInterval { get; set; }
        public List<int[]>? PageRanges { get; set; }
        public string? OutputFileNamePattern { get; set; } = "page_{0}.pdf";
    }

    public enum SplitMode
    {
        ByPage,
        ByRange,
        ByInterval,
        ExtractPages
    }
}
