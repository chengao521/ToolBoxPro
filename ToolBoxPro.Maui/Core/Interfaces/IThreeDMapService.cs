using ToolBoxPro.Core.Models;

namespace ToolBoxPro.Core.Interfaces
{
    public interface IThreeDMapService
    {
        Task<OperationResult<string>> GenerateMapAsync(ThreeDMapOptions options);
        string GenerateHtml(ThreeDMapOptions options);
    }

    public class ThreeDMapOptions
    {
        public string Title { get; set; } = "3D 中国地图";
        public string GeoJsonUrl { get; set; } = "https://geo.datav.aliyun.com/areas_v3/bound/100000_full.json";
        public string OutputPath { get; set; } = string.Empty;
        public string ProjectionCenterLongitude { get; set; } = "104";
        public string ProjectionCenterLatitude { get; set; } = "37.5";
        public int ExtrudeDepth { get; set; } = 10;
        public string TopColor { get; set; } = "#0a2a3a";
        public string SideColor { get; set; } = "#0d3b4f";
        public string BorderColor { get; set; } = "#ffffff";
        public string HighlightColor { get; set; } = "#ffeb3b";
        public bool ShowStars { get; set; } = true;
        public bool ShowLabels { get; set; } = false;
    }
}
