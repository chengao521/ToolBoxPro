using System.Windows.Controls;
using ToolBoxPro.ViewModels;

namespace ToolBoxPro.Views
{
    public partial class ThreeDMapView : UserControl
    {
        public ThreeDMapView(ThreeDMapViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
