using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.IO;
using System.Windows;
using ToolBoxPro.Views;

namespace ToolBoxPro.ViewModels
{
    public partial class ScrollCaptureViewModel : ObservableObject
    {
        private readonly Services.ScrollCaptureService _captureService;
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isCapturing;
        private bool _shouldContinue = true;

        [ObservableProperty]
        private Rect _captureRect = new Rect(0, 0, 800, 600);

        [ObservableProperty]
        private int _captureLeft;

        [ObservableProperty]
        private int _captureTop;

        [ObservableProperty]
        private int _captureWidth = 800;

        [ObservableProperty]
        private int _captureHeight = 600;

        [ObservableProperty]
        private int _scrollInterval = 200;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _statusMessage = "准备就绪";

        [ObservableProperty]
        private int _frameCount;

        [ObservableProperty]
        private string? _outputPath;

        [ObservableProperty]
        private bool _autoStopOnNoChange = true;

        [ObservableProperty]
        private bool _manualScrollMode = false;

        [ObservableProperty]
        private int _manualCaptureCount = 0;

        public ScrollCaptureViewModel()
        {
            _captureService = new Services.ScrollCaptureService();
            _captureService.StatusChanged += status => StatusMessage = status;
            _captureService.CaptureProgress += count => FrameCount = count;
        }

        [RelayCommand]
        private void SelectCaptureRegion()
        {
            var mainWindow = Application.Current.MainWindow;
            
            try
            {
                // 隐藏主窗口
                mainWindow?.Hide();

                // 延迟确保窗口隐藏
                System.Threading.Thread.Sleep(200);

                // 创建全屏透明窗口用于选择区域
                var selectionWindow = new RegionSelectionWindow();
                if (selectionWindow.ShowDialog() == true && selectionWindow.SelectedRegion.HasValue)
                {
                    var region = selectionWindow.SelectedRegion.Value;
                    CaptureLeft = (int)region.Left;
                    CaptureTop = (int)region.Top;
                    CaptureWidth = (int)region.Width;
                    CaptureHeight = (int)region.Height;
                    StatusMessage = $"已选择区域: {region.Width} x {region.Height}";
                }
            }
            finally
            {
                // 恢复主窗口
                mainWindow?.Show();
            }
        }

        [RelayCommand]
        private async Task StartCapture()
        {
            if (_isCapturing) return;

            // 验证参数
            if (CaptureWidth <= 0 || CaptureHeight <= 0)
            {
                MessageBox.Show("请输入有效的截屏区域尺寸", "参数错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _isCapturing = true;
            _shouldContinue = true;
            IsBusy = true;
            FrameCount = 0;

            CaptureRect = new Rect(CaptureLeft, CaptureTop, CaptureWidth, CaptureHeight);
            _captureService.ScrollIntervalMs = ScrollInterval;
            _captureService.AutoStopOnNoChange = AutoStopOnNoChange;
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                if (!ManualScrollMode)
                {
                    // 自动滚动模式
                    StatusMessage = "正在准备截屏...";

                    // 隐藏当前窗口以免被截到
                    var mainWindow = Application.Current.MainWindow;
                    if (mainWindow != null)
                    {
                        mainWindow.WindowState = WindowState.Minimized;
                        await Task.Delay(500);
                    }

                    var result = await _captureService.CaptureScrollAsync(
                        CaptureRect,
                        () => _shouldContinue,
                        _cancellationTokenSource.Token);

                    ProcessCaptureResult(result);
                }
                else
                {
                    StatusMessage = "请将目标窗口调整到截屏区域内，点击「捕获下一帧」逐帧捕获，完成后点击「停止截屏」";
                    await Task.Delay(100);
                    _captureService.CaptureManualFrame(CaptureRect);
                    FrameCount = 1;
                    StatusMessage = $"已捕获第 {FrameCount} 帧，手动滚动目标窗口后点击「捕获下一帧」";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"错误: {ex.Message}";
                MessageBox.Show($"截图失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (!ManualScrollMode)
                {
                    _isCapturing = false;
                    IsBusy = false;

                    // 恢复窗口
                    var mainWindow = Application.Current.MainWindow;
                    if (mainWindow != null)
                    {
                        mainWindow.WindowState = WindowState.Normal;
                    }
                }
            }
        }

        [RelayCommand]
        private async Task CaptureNextFrame()
        {
            if (!_isCapturing || !ManualScrollMode) return;

            try
            {
                await Task.Delay(100);
                _captureService.CaptureManualFrame(CaptureRect);
                FrameCount++;
                StatusMessage = $"已捕获第 {FrameCount} 帧，继续滚动后点击继续";
            }
            catch (Exception ex)
            {
                StatusMessage = $"捕获失败: {ex.Message}";
            }
        }

        [RelayCommand]
        private void StopCapture()
        {
            if (!_isCapturing) return;

            _shouldContinue = false;
            _cancellationTokenSource?.Cancel();

            if (ManualScrollMode)
            {
                // 手动模式：拼接所有帧
                var result = _captureService.FinishManualCapture();
                ProcessCaptureResult(result);

                _isCapturing = false;
                IsBusy = false;

                // 恢复窗口
                var mainWindow = Application.Current.MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.WindowState = WindowState.Normal;
                }
            }
            else
            {
                StatusMessage = "正在停止...";
            }
        }

        private void ProcessCaptureResult(string? result)
        {
            if (result != null)
            {
                OutputPath = result;
                StatusMessage = $"截图完成！共 {FrameCount} 帧";

                // 自动打开文件
                if (File.Exists(result))
                {
                    Process.Start(new ProcessStartInfo(result) { UseShellExecute = true });
                }
            }
            else
            {
                StatusMessage = "截图已取消或失败";
            }
        }

        [RelayCommand]
        private void OpenOutputFolder()
        {
            if (string.IsNullOrEmpty(OutputPath) || !File.Exists(OutputPath)) return;

            string folder = Path.GetDirectoryName(OutputPath)!;
            Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{OutputPath}\"") { UseShellExecute = true });
        }

        [RelayCommand]
        private void Reset()
        {
            CaptureLeft = 0;
            CaptureTop = 0;
            CaptureWidth = 800;
            CaptureHeight = 600;
            ScrollInterval = 200;
            FrameCount = 0;
            StatusMessage = "准备就绪";
            OutputPath = null;
            ManualScrollMode = false;
        }
    }
}
