using System.Windows.Controls;
using ToolBoxPro.ViewModels;

namespace ToolBoxPro.Views
{
    public partial class ProteinViewerView : UserControl
    {
        public ProteinViewerView(ProteinViewerViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
