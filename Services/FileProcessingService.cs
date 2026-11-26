using ToolBoxPro.Core.Interfaces;
using ToolBoxPro.Core.Models;
using System.IO;

namespace ToolBoxPro.Services
{
    public class FileProcessingService : IFileService
    {
        public async Task<OperationResult<int>> BatchRenameAsync(BatchRenameOptions options)
        {
            try
            {
                if (!Directory.Exists(options.Directory))
                {
                    return OperationResult<int>.Fail("目录不存在");
                }

                return await Task.Run(() =>
                {
                    var searchOption = options.IncludeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                    var files = Directory.GetFiles(options.Directory, options.SearchPattern, searchOption);
                    int renamedCount = 0;
                    int index = options.StartIndex;

                    foreach (var file in files)
                    {
                        try
                        {
                            var directory = Path.GetDirectoryName(file);
                            var fileName = Path.GetFileNameWithoutExtension(file);
                            var extension = Path.GetExtension(file);
                            string newName;

                            switch (options.Mode)
                            {
                                case RenameMode.Sequential:
                                    newName = options.NewNamePattern
                                        .Replace("{name}", fileName)
                                        .Replace("{index}", index.ToString())
                                        .Replace("{ext}", extension);
                                    index++;
                                    break;

                                case RenameMode.FindReplace:
                                    if (string.IsNullOrEmpty(options.FindText)) continue;
                                    newName = fileName.Replace(options.FindText, options.ReplaceText ?? "") + extension;
                                    break;

                                case RenameMode.AddPrefix:
                                    newName = $"{options.FindText}{fileName}{extension}";
                                    break;

                                case RenameMode.AddSuffix:
                                    newName = $"{fileName}{options.FindText}{extension}";
                                    break;

                                case RenameMode.ChangeExtension:
                                    newName = $"{fileName}{options.ReplaceText}";
                                    break;

                                default:
                                    continue;
                            }

                            var newPath = Path.Combine(directory!, newName);
                            
                            // 避免重命名冲突
                            if (File.Exists(newPath) && !string.Equals(file, newPath, StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }

                            File.Move(file, newPath);
                            renamedCount++;
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"重命名文件 {file} 失败: {ex.Message}");
                        }
                    }

                    return OperationResult<int>.Ok(renamedCount, $"成功重命名 {renamedCount} 个文件");
                });
            }
            catch (Exception ex)
            {
                return OperationResult<int>.Fail($"批量重命名失败: {ex.Message}", ex);
            }
        }

        public async Task<OperationResult<List<string>>> FindDuplicatesAsync(string directory, bool includeSubdirectories = false)
        {
            try
            {
                if (!Directory.Exists(directory))
                {
                    return OperationResult<List<string>>.Fail("目录不存在");
                }

                return await Task.Run(() =>
                {
                    var searchOption = includeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                    var files = Directory.GetFiles(directory, "*.*", searchOption);
                    var fileHashes = new Dictionary<string, List<string>>();

                    foreach (var file in files)
                    {
                        try
                        {
                            var hash = CalculateFileHash(file);
                            if (!fileHashes.ContainsKey(hash))
                            {
                                fileHashes[hash] = new List<string>();
                            }
                            fileHashes[hash].Add(file);
                        }
                        catch
                        {
                            // 忽略无法读取的文件
                        }
                    }

                    var duplicates = fileHashes
                        .Where(x => x.Value.Count > 1)
                        .SelectMany(x => x.Value)
                        .ToList();

                    return OperationResult<List<string>>.Ok(duplicates, $"找到 {duplicates.Count} 个重复文件");
                });
            }
            catch (Exception ex)
            {
                return OperationResult<List<string>>.Fail($"查找重复文件失败: {ex.Message}", ex);
            }
        }

        public async Task<OperationResult<long>> CalculateDirectorySizeAsync(string directory)
        {
            try
            {
                if (!Directory.Exists(directory))
                {
                    return OperationResult<long>.Fail("目录不存在");
                }

                return await Task.Run(() =>
                {
                    long size = CalculateDirectorySize(directory);
                    return OperationResult<long>.Ok(size, $"目录大小: {FormatFileSize(size)}");
                });
            }
            catch (Exception ex)
            {
                return OperationResult<long>.Fail($"计算目录大小失败: {ex.Message}", ex);
            }
        }

        private long CalculateDirectorySize(string directory)
        {
            long size = 0;
            var files = Directory.GetFiles(directory);
            
            foreach (var file in files)
            {
                try
                {
                    var info = new FileInfo(file);
                    size += info.Length;
                }
                catch
                {
                    // 忽略无法访问的文件
                }
            }

            var subdirectories = Directory.GetDirectories(directory);
            foreach (var subdir in subdirectories)
            {
                try
                {
                    size += CalculateDirectorySize(subdir);
                }
                catch
                {
                    // 忽略无法访问的目录
                }
            }

            return size;
        }

        private string CalculateFileHash(string filePath)
        {
            using var stream = File.OpenRead(filePath);
            using var md5 = System.Security.Cryptography.MD5.Create();
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double size = bytes;
            
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }
            
            return $"{size:0.##} {sizes[order]}";
        }
    }
}
