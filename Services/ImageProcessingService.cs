using System.Drawing;
using System.Drawing.Imaging;
using ToolBoxPro.Core.Interfaces;
using ToolBoxPro.Core.Models;
using System.IO;
using DrawingImageFormat = System.Drawing.Imaging.ImageFormat;
using AppImageFormat = ToolBoxPro.Core.Interfaces.ImageFormat;

namespace ToolBoxPro.Services
{
    public class ImageProcessingService : IImageService
    {
        public async Task<OperationResult<List<string>>> ConvertImagesAsync(ImageConvertOptions options)
        {
            try
            {
                if (options.SourceFiles == null || options.SourceFiles.Count == 0)
                {
                    return OperationResult<List<string>>.Fail("没有选择源文件");
                }

                if (!Directory.Exists(options.OutputDirectory))
                {
                    Directory.CreateDirectory(options.OutputDirectory);
                }

                return await Task.Run(() =>
                {
                    var outputFiles = new List<string>();
                    var format = GetImageFormat(options.TargetFormat);
                    var encoder = GetEncoder(format);

                    foreach (var sourceFile in options.SourceFiles)
                    {
                        if (!File.Exists(sourceFile)) continue;

                        try
                        {
                            using var image = Image.FromFile(sourceFile);
                            var (width, height) = CalculateDimensions(image, options);
                            
                            using var resizedImage = new Bitmap(width, height);
                            using (var graphics = Graphics.FromImage(resizedImage))
                            {
                                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                                graphics.DrawImage(image, 0, 0, width, height);
                            }

                            var fileName = Path.GetFileNameWithoutExtension(sourceFile);
                            var ext = GetExtension(options.TargetFormat);
                            var outputPath = Path.Combine(options.OutputDirectory, $"{fileName}{ext}");
                            
                            int counter = 1;
                            while (File.Exists(outputPath))
                            {
                                outputPath = Path.Combine(options.OutputDirectory, $"{fileName}_{counter}{ext}");
                                counter++;
                            }

                            if (encoder != null && options.Quality.HasValue && format == DrawingImageFormat.Jpeg)
                            {
                                var encoderParams = new EncoderParameters(1);
                                encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, options.Quality.Value);
                                resizedImage.Save(outputPath, encoder, encoderParams);
                            }
                            else
                            {
                                resizedImage.Save(outputPath, format);
                            }

                            outputFiles.Add(outputPath);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"处理文件 {sourceFile} 失败: {ex.Message}");
                        }
                    }

                    return OperationResult<List<string>>.Ok(outputFiles, $"成功转换 {outputFiles.Count} 个文件");
                });
            }
            catch (Exception ex)
            {
                return OperationResult<List<string>>.Fail($"转换图片失败: {ex.Message}", ex);
            }
        }

        public async Task<OperationResult<bool>> ValidateImageAsync(string filePath)
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
                        using var image = Image.FromFile(filePath);
                        return OperationResult<bool>.Ok(true, "图片文件有效");
                    }
                    catch
                    {
                        return OperationResult<bool>.Ok(false, "图片文件无效或已损坏");
                    }
                });
            }
            catch (Exception ex)
            {
                return OperationResult<bool>.Fail($"验证图片失败: {ex.Message}", ex);
            }
        }

        public async Task<OperationResult<ImageInfo>> GetImageInfoAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return OperationResult<ImageInfo>.Fail("文件不存在");
                }

                return await Task.Run(() =>
                {
                    using var image = Image.FromFile(filePath);
                    var fileInfo = new FileInfo(filePath);
                    
                    var info = new ImageInfo
                    {
                        Width = image.Width,
                        Height = image.Height,
                        Format = image.RawFormat.ToString(),
                        FileSize = fileInfo.Length
                    };

                    return OperationResult<ImageInfo>.Ok(info);
                });
            }
            catch (Exception ex)
            {
                return OperationResult<ImageInfo>.Fail($"获取图片信息失败: {ex.Message}", ex);
            }
        }

        private (int width, int height) CalculateDimensions(Image image, ImageConvertOptions options)
        {
            int width = image.Width;
            int height = image.Height;

            if (options.MaxWidth.HasValue || options.MaxHeight.HasValue)
            {
                double ratio = (double)width / height;

                if (options.MaxWidth.HasValue && width > options.MaxWidth.Value)
                {
                    width = options.MaxWidth.Value;
                    if (options.KeepAspectRatio)
                    {
                        height = (int)(width / ratio);
                    }
                }

                if (options.MaxHeight.HasValue && height > options.MaxHeight.Value)
                {
                    height = options.MaxHeight.Value;
                    if (options.KeepAspectRatio)
                    {
                        width = (int)(height * ratio);
                    }
                }
            }

            return (width, height);
        }

        private DrawingImageFormat GetImageFormat(AppImageFormat format)
        {
            return format switch
            {
                AppImageFormat.Jpeg => DrawingImageFormat.Jpeg,
                AppImageFormat.Png => DrawingImageFormat.Png,
                AppImageFormat.Bmp => DrawingImageFormat.Bmp,
                AppImageFormat.Gif => DrawingImageFormat.Gif,
                _ => DrawingImageFormat.Jpeg
            };
        }

        private string GetExtension(AppImageFormat format)
        {
            return format switch
            {
                AppImageFormat.Jpeg => ".jpg",
                AppImageFormat.Png => ".png",
                AppImageFormat.Bmp => ".bmp",
                AppImageFormat.Gif => ".gif",
                AppImageFormat.Webp => ".webp",
                _ => ".jpg"
            };
        }

        private ImageCodecInfo? GetEncoder(DrawingImageFormat format)
        {
            var codecs = ImageCodecInfo.GetImageEncoders();
            return codecs.FirstOrDefault(codec => codec.FormatID == format.Guid);
        }
    }
}
