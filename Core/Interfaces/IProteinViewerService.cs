using ToolBoxPro.Core.Models;

namespace ToolBoxPro.Core.Interfaces
{
    public interface IProteinViewerService
    {
        Task<OperationResult<string>> GenerateViewerAsync(ProteinViewerOptions options);
        string GenerateHtml(ProteinViewerOptions options);
    }

    public class ProteinViewerOptions
    {
        public string Title { get; set; } = "蛋白质 3D 结构查看器";
        public string PdbFileUrl { get; set; } = "https://raw.githubusercontent.com/martin-groenhout/atomic-data/master/1TQN.pdb";
        public string OutputPath { get; set; } = string.Empty;
        public bool ShowBallAndStick { get; set; } = true;
        public bool ShowRibbon { get; set; } = true;
        public bool ShowLabels { get; set; } = false;
        public string HelixColor { get; set; } = "#FF00FF";
        public string SheetColor { get; set; } = "#FFD700";
        public string CoilColor { get; set; } = "#808080";
        public string BackgroundColor { get; set; } = "#0a0e1a";
        public int AtomRadiusScale { get; set; } = 10;
        public int RibbonWidth { get; set; } = 5;
    }
}
