using ToolBoxPro.ViewModels;
using System.Windows.Controls;

namespace ToolBoxPro.Views
{
    public partial class ImageConverterView : UserControl
    {
        public ImageConverterView(ImageConverterViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
