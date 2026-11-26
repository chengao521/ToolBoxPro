using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;

namespace ToolBoxPro.Services
{
    public class ScrollCaptureService
    {
        #region Win32 API

        [DllImport("user32.dll")]
        private static extern IntPtr WindowFromPoint(POINT point);

        [DllImport("user32.dll")]
        private static extern IntPtr GetAncestor(IntPtr hwnd, uint gaFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr ChildWindowFromPoint(IntPtr hwndParent, POINT point);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight,
            IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        private const uint WM_MOUSEWHEEL = 0x020A;
        private const uint GA_ROOT = 2;
        private const uint SRCCOPY = 0x00CC0020;
        private const int WHEEL_DELTA = 120;

        #endregion

        public event Action<string>? StatusChanged;
        public event Action<int>? CaptureProgress;

        public int ScrollIntervalMs { get; set; } = 200;
        public int ScrollDelta { get; set; } = -WHEEL_DELTA;
        public bool AutoStopOnNoChange { get; set; } = true;

        private List<Bitmap> _manualFrames = new();

        public async Task<string?> CaptureScrollAsync(Rect captureRect, Func<bool> shouldContinue, CancellationToken cancellationToken = default)
        {
            try
            {
                var tiles = new List<Bitmap>();
                var screenWidth = (int)captureRect.Width;
                var screenHeight = (int)captureRect.Height;

                StatusChanged?.Invoke("正在捕获首帧...");

                // 捕获第一帧
                var firstTile = CaptureRegion((int)captureRect.X, (int)captureRect.Y, screenWidth, screenHeight);
                if (firstTile == null) return null;
                tiles.Add(firstTile);
                CaptureProgress?.Invoke(tiles.Count);

                StatusChanged?.Invoke("开始滚动截屏，点击停止截屏可中止...");
                while (shouldContinue() && !cancellationToken.IsCancellationRequested)
                {
                    // 滚动
                    var centerX = (int)(captureRect.X + captureRect.Width / 2);
                    var centerY = (int)(captureRect.Y + captureRect.Height / 2);

                    SimulateWheelAt(centerX, centerY);

                    // 等待渲染
                    await Task.Delay(ScrollIntervalMs, cancellationToken);

                    // 捕获当前帧
                    var tile = CaptureRegion((int)captureRect.X, (int)captureRect.Y, screenWidth, screenHeight);
                    if (tile == null) break;

                    // 检查是否与上一帧有显著差异
                    if (IsSignificantlyDifferent(tiles[tiles.Count - 1], tile))
                    {
                        tiles.Add(tile);
                        CaptureProgress?.Invoke(tiles.Count);
                        StatusChanged?.Invoke($"已捕获 {tiles.Count} 帧...");
                    }
                    else
                    {
                        tile.Dispose();
                        if (AutoStopOnNoChange)
                        {
                            StatusChanged?.Invoke("检测到画面无变化，滚动结束");
                            break;
                        }
                    }
                }

                if (tiles.Count == 0) return null;

                StatusChanged?.Invoke("正在拼接图片...");

                // 拼接所有帧
                var result = StitchTiles(tiles);

                // 清理
                foreach (var tile in tiles)
                {
                    tile.Dispose();
                }

                StatusChanged?.Invoke("截图完成");
                return result;
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke($"错误: {ex.Message}");
                return null;
            }
        }

        private void SimulateWheelAt(int x, int y)
        {
            var point = new POINT { X = x, Y = y };
            IntPtr hwnd = WindowFromPoint(point);

            if (hwnd == IntPtr.Zero)
                hwnd = GetDesktopWindow();

            // 获取根窗口
            hwnd = GetAncestor(hwnd, GA_ROOT);

            // 循环找到最深层的子窗口
            int loopCount = 0;
            IntPtr lastHwnd = hwnd;
            while (loopCount < 10)
            {
                var child = ChildWindowFromPoint(hwnd, point);
                if (child == IntPtr.Zero || child == hwnd || child == lastHwnd)
                    break;
                lastHwnd = hwnd;
                hwnd = child;
                loopCount++;
            }

            // 发送滚轮消息
            IntPtr wParam = (IntPtr)((ScrollDelta << 16) & 0xFFFFFFFF);
            IntPtr lParam = (IntPtr)((y << 16) | (x & 0xFFFF));

            SendMessage(hwnd, WM_MOUSEWHEEL, wParam, lParam);
        }

        private Bitmap? CaptureRegion(int x, int y, int width, int height)
        {
            try
            {
                var desktop = GetDesktopWindow();
                IntPtr desktopDC = GetWindowDC(desktop);
                IntPtr memDC = CreateCompatibleDC(desktopDC);
                IntPtr hBitmap = CreateCompatibleBitmap(desktopDC, width, height);
                IntPtr oldBitmap = SelectObject(memDC, hBitmap);

                BitBlt(memDC, 0, 0, width, height, desktopDC, x, y, SRCCOPY);

                SelectObject(memDC, oldBitmap);

                var bitmap = Image.FromHbitmap(hBitmap);
                bitmap.SetResolution(96, 96);

                DeleteObject(hBitmap);
                DeleteDC(memDC);
                ReleaseDC(desktop, desktopDC);

                return new Bitmap(bitmap);
            }
            catch
            {
                return null;
            }
        }

        private bool IsSignificantlyDifferent(Bitmap prev, Bitmap next)
        {
            if (prev.Width != next.Width || prev.Height != next.Height)
                return true;

            int diffCount = 0;
            int totalSamples = 100;
            int stepX = Math.Max(1, prev.Width / 10);
            int stepY = Math.Max(1, prev.Height / 10);

            for (int y = 0; y < prev.Height; y += stepY)
            {
                for (int x = 0; x < prev.Width; x += stepX)
                {
                    var c1 = ((Bitmap)prev).GetPixel(x, y);
                    var c2 = ((Bitmap)next).GetPixel(x, y);

                    int diff = Math.Abs(c1.R - c2.R) + Math.Abs(c1.G - c2.G) + Math.Abs(c1.B - c2.B);
                    if (diff > 30)
                        diffCount++;
                }
            }

            return diffCount > totalSamples * 0.05;
        }

        private string? StitchTiles(List<Bitmap> tiles)
        {
            if (tiles.Count == 0) return null;
            if (tiles.Count == 1) return SaveBitmap(tiles[0]);

            int width = tiles[0].Width;
            int totalHeight = tiles[0].Height;

            // 计算总高度
            var overlaps = new List<int>();
            for (int i = 0; i < tiles.Count - 1; i++)
            {
                int overlap = EstimateOverlap(tiles[i], tiles[i + 1]);
                overlaps.Add(overlap);
                totalHeight += tiles[i + 1].Height - overlap;
            }

            // 创建结果图像
            var result = new Bitmap(width, totalHeight);
            var g = Graphics.FromImage(result);
            g.Clear(Color.White);

            int yOffset = 0;
            g.DrawImage(tiles[0], 0, 0);

            yOffset += tiles[0].Height;

            for (int i = 0; i < tiles.Count - 1; i++)
            {
                int overlap = overlaps[i];
                int newY = yOffset - overlap;
                g.DrawImage(tiles[i + 1], 0, newY);
                yOffset = newY + tiles[i + 1].Height;
            }

            g.Dispose();
            return SaveBitmap(result);
        }

        private int EstimateOverlap(Bitmap top, Bitmap bottom)
        {
            int maxSearchHeight = Math.Min(top.Height, bottom.Height) / 2;
            int minSearchHeight = Math.Min(20, maxSearchHeight);
            int bestOverlap = 0;
            long bestScore = long.MaxValue;

            for (int overlap = minSearchHeight; overlap <= maxSearchHeight; overlap += 5)
            {
                long score = CalculateDifference(top, bottom, top.Height - overlap, 0, overlap);
                if (score < bestScore)
                {
                    bestScore = score;
                    bestOverlap = overlap;
                }
            }

            return bestOverlap;
        }

        private long CalculateDifference(Bitmap top, Bitmap bottom, int topY, int bottomY, int sampleHeight)
        {
            long diff = 0;
            int sampleStep = 10;

            for (int y = 0; y < sampleHeight; y += sampleStep)
            {
                for (int x = 0; x < top.Width; x += sampleStep)
                {
                    var c1 = top.GetPixel(x, topY + y);
                    var c2 = bottom.GetPixel(x, bottomY + y);
                    diff += Math.Abs(c1.R - c2.R) + Math.Abs(c1.G - c2.G) + Math.Abs(c1.B - c2.B);
                }
            }

            return diff;
        }

        private string SaveBitmap(Bitmap bitmap)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), $"scroll_capture_{DateTime.Now:yyyyMMddHHmmss}.png");
            bitmap.Save(tempPath, ImageFormat.Png);
            return tempPath;
        }

        #region 手动捕获模式

        public void CaptureManualFrame(Rect captureRect)
        {
            var bitmap = CaptureRegion((int)captureRect.X, (int)captureRect.Y, 
                                      (int)captureRect.Width, (int)captureRect.Height);
            if (bitmap != null)
            {
                _manualFrames.Add(bitmap);
            }
        }

        public string? FinishManualCapture()
        {
            if (_manualFrames.Count == 0) return null;

            try
            {
                StatusChanged?.Invoke("正在拼接图片...");
                var result = StitchTiles(_manualFrames);
                
                foreach (var frame in _manualFrames)
                {
                    frame.Dispose();
                }
                _manualFrames.Clear();

                StatusChanged?.Invoke("截图完成");
                return result;
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke($"错误: {ex.Message}");
                return null;
            }
        }

        public void ClearManualFrames()
        {
            foreach (var frame in _manualFrames)
            {
                frame.Dispose();
            }
            _manualFrames.Clear();
        }

        #endregion
    }
}
