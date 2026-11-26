using ToolBoxPro.ViewModels;

namespace ToolBoxPro.Views
{
    public partial class ThreeDMapPage : ContentPage
    {
        public ThreeDMapPage(ThreeDMapViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
