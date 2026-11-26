using ToolBoxPro.ViewModels;
using System.Windows.Controls;

namespace ToolBoxPro.Views
{
    public partial class ScrollCaptureView : UserControl
    {
        public ScrollCaptureView(ScrollCaptureViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
