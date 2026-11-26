using ToolBoxPro.Core.Models;

namespace ToolBoxPro.Core.Interfaces
{
    public interface IImageService
    {
        Task<OperationResult<List<string>>> ConvertImagesAsync(ImageConvertOptions options);
        Task<OperationResult<bool>> ValidateImageAsync(string filePath);
        Task<OperationResult<ImageInfo>> GetImageInfoAsync(string filePath);
    }

    public class ImageConvertOptions
    {
        public List<string> SourceFiles { get; set; } = new();
        public string OutputDirectory { get; set; } = string.Empty;
        public ImageFormat TargetFormat { get; set; } = ImageFormat.Jpeg;
        public int? Quality { get; set; } = 90;
        public int? MaxWidth { get; set; }
        public int? MaxHeight { get; set; }
        public bool KeepAspectRatio { get; set; } = true;
    }

    public enum ImageFormat
    {
        Jpeg,
        Png,
        Bmp,
        Gif,
        Webp
    }

    public class ImageInfo
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public string Format { get; set; } = string.Empty;
        public long FileSize { get; set; }
    }
}
