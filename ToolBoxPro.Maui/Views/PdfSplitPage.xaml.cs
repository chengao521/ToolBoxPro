using ToolBoxPro.ViewModels;

namespace ToolBoxPro.Views
{
    public partial class PdfSplitPage : ContentPage
    {
        public PdfSplitPage(PdfSplitViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
