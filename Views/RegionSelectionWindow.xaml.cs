using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ToolBoxPro.Views
{
    public partial class RegionSelectionWindow : Window
    {
        private Point _startPoint;
        private bool _isSelecting;

        public Rect? SelectedRegion { get; private set; }

        public RegionSelectionWindow()
        {
            InitializeComponent();
            
            // 设置窗口为全屏
            WindowState = WindowState.Maximized;
            SelectionCanvas.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            SelectionCanvas.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _startPoint = e.GetPosition(SelectionCanvas);
                _isSelecting = true;
                SelectionRect.Visibility = Visibility.Visible;
                SelectionRect.Width = 0;
                SelectionRect.Height = 0;
                Canvas.SetLeft(SelectionRect, _startPoint.X);
                Canvas.SetTop(SelectionRect, _startPoint.Y);
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isSelecting) return;

            Point currentPoint = e.GetPosition(SelectionCanvas);
            
            double x = Math.Min(_startPoint.X, currentPoint.X);
            double y = Math.Min(_startPoint.Y, currentPoint.Y);
            double width = Math.Abs(currentPoint.X - _startPoint.X);
            double height = Math.Abs(currentPoint.Y - _startPoint.Y);

            Canvas.SetLeft(SelectionRect, x);
            Canvas.SetTop(SelectionRect, y);
            SelectionRect.Width = width;
            SelectionRect.Height = height;

            // 更新尺寸显示
            SizeLabel.Visibility = Visibility.Visible;
            SizeLabel.Text = $"宽: {(int)width} x 高: {(int)height}";
            Canvas.SetLeft(SizeLabel, x + 5);
            Canvas.SetTop(SizeLabel, y - 25);
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && _isSelecting)
            {
                _isSelecting = false;
                
                if (SelectionRect.Width > 10 && SelectionRect.Height > 10)
                {
                    var topLeft = SelectionCanvas.PointToScreen(new Point(Canvas.GetLeft(SelectionRect), Canvas.GetTop(SelectionRect)));
                    var bottomRight = SelectionCanvas.PointToScreen(new Point(Canvas.GetLeft(SelectionRect) + SelectionRect.Width, Canvas.GetTop(SelectionRect) + SelectionRect.Height));

                    SelectedRegion = new Rect(
                        topLeft.X,
                        topLeft.Y,
                        bottomRight.X - topLeft.X,
                        bottomRight.Y - topLeft.Y);
                    
                    DialogResult = true;
                }
                else
                {
                    // 区域太小，取消选择
                    SelectionRect.Visibility = Visibility.Hidden;
                    SizeLabel.Visibility = Visibility.Hidden;
                }
            }
        }

        private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 右键取消选择
            DialogResult = false;
            Close();
        }
    }
}
