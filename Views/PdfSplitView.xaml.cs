using ToolBoxPro.ViewModels;
using System.Windows.Controls;

namespace ToolBoxPro.Views
{
    public partial class PdfSplitView : UserControl
    {
        public PdfSplitView(PdfSplitViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
