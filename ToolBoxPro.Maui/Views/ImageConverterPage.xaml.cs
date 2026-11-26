using ToolBoxPro.ViewModels;

namespace ToolBoxPro.Views
{
    public partial class ImageConverterPage : ContentPage
    {
        public ImageConverterPage(ImageConverterViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
