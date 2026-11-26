using ToolBoxPro.ViewModels;
using System.Windows.Controls;

namespace ToolBoxPro.Views
{
    public partial class HomeView : UserControl
    {
        public HomeView(HomeViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
