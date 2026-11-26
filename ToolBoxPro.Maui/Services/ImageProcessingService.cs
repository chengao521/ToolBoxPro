using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using System.IO;

namespace ToolBoxPro.Services
{
    public class ImageProcessingService : Core.Interfaces.IImageService
    {
        public async Task<Core.Models.OperationResult<List<string>>> ConvertImagesAsync(Core.Interfaces.ImageConvertOptions options)
        {
            try
            {
                if (options.SourceFiles == null || options.SourceFiles.Count == 0)
                {
                    return Core.Models.OperationResult<List<string>>.Fail("没有选择源文件");
                }

                if (!Directory.Exists(options.OutputDirectory))
                {
                    Directory.CreateDirectory(options.OutputDirectory);
                }

                return await Task.Run(() =>
                {
                    var outputFiles = new List<string>();

                    foreach (var sourceFile in options.SourceFiles)
                    {
                        if (!File.Exists(sourceFile)) continue;

                        try
                        {
                            using var image = SixLabors.ImageSharp.Image.Load(sourceFile);
                            var (width, height) = CalculateDimensions(image, options);

                            image.Mutate(x => x.Resize(width, height));

                            var fileName = Path.GetFileNameWithoutExtension(sourceFile);
                            var ext = GetExtension(options.TargetFormat);
                            var outputPath = Path.Combine(options.OutputDirectory, $"{fileName}{ext}");

                            int counter = 1;
                            while (File.Exists(outputPath))
                            {
                                outputPath = Path.Combine(options.OutputDirectory, $"{fileName}_{counter}{ext}");
                                counter++;
                            }

                            SaveImage(image, outputPath, options.TargetFormat, options.Quality);
                            outputFiles.Add(outputPath);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"处理文件 {sourceFile} 失败: {ex.Message}");
                        }
                    }

                    return Core.Models.OperationResult<List<string>>.Ok(outputFiles, $"成功转换 {outputFiles.Count} 个文件");
                });
            }
            catch (Exception ex)
            {
                return Core.Models.OperationResult<List<string>>.Fail($"转换图片失败: {ex.Message}", ex);
            }
        }

        public async Task<Core.Models.OperationResult<bool>> ValidateImageAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return Core.Models.OperationResult<bool>.Fail("文件不存在");
                }

                return await Task.Run(() =>
                {
                    try
                    {
                        using var image = SixLabors.ImageSharp.Image.Load(filePath);
                        return Core.Models.OperationResult<bool>.Ok(true, "图片文件有效");
                    }
                    catch
                    {
                        return Core.Models.OperationResult<bool>.Ok(false, "图片文件无效或已损坏");
                    }
                });
            }
            catch (Exception ex)
            {
                return Core.Models.OperationResult<bool>.Fail($"验证图片失败: {ex.Message}", ex);
            }
        }

        public async Task<Core.Models.OperationResult<Core.Interfaces.ImageInfo>> GetImageInfoAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return Core.Models.OperationResult<Core.Interfaces.ImageInfo>.Fail("文件不存在");
                }

                return await Task.Run(() =>
                {
                    using var image = SixLabors.ImageSharp.Image.Load(filePath);
                    var fileInfo = new FileInfo(filePath);

                    var info = new Core.Interfaces.ImageInfo
                    {
                        Width = image.Width,
                        Height = image.Height,
                        Format = GetImageFormatName(filePath, image),
                        FileSize = fileInfo.Length
                    };

                    return Core.Models.OperationResult<Core.Interfaces.ImageInfo>.Ok(info);
                });
            }
            catch (Exception ex)
            {
                return Core.Models.OperationResult<Core.Interfaces.ImageInfo>.Fail($"获取图片信息失败: {ex.Message}", ex);
            }
        }

        private (int width, int height) CalculateDimensions(SixLabors.ImageSharp.Image image, Core.Interfaces.ImageConvertOptions options)
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

        private void SaveImage(SixLabors.ImageSharp.Image image, string outputPath, Core.Interfaces.ImageFormat format, int? quality)
        {
            switch (format)
            {
                case Core.Interfaces.ImageFormat.Jpeg:
                    var jpegOptions = new JpegEncoder { Quality = quality ?? 90 };
                    image.Save(outputPath, jpegOptions);
                    break;
                case Core.Interfaces.ImageFormat.Png:
                    image.Save(outputPath);
                    break;
                case Core.Interfaces.ImageFormat.Bmp:
                    image.Save(outputPath);
                    break;
                case Core.Interfaces.ImageFormat.Gif:
                    image.Save(outputPath);
                    break;
                case Core.Interfaces.ImageFormat.Webp:
                    image.Save(outputPath);
                    break;
                default:
                    image.Save(outputPath);
                    break;
            }
        }

        private string GetExtension(Core.Interfaces.ImageFormat format)
        {
            return format switch
            {
                Core.Interfaces.ImageFormat.Jpeg => ".jpg",
                Core.Interfaces.ImageFormat.Png => ".png",
                Core.Interfaces.ImageFormat.Bmp => ".bmp",
                Core.Interfaces.ImageFormat.Gif => ".gif",
                Core.Interfaces.ImageFormat.Webp => ".webp",
                _ => ".jpg"
            };
        }

        private static string GetImageFormatName(string filePath, SixLabors.ImageSharp.Image image)
        {
            return Path.GetExtension(filePath).ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".bmp" => "image/bmp",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "unknown"
            };
        }
    }
}
