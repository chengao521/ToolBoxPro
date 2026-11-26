using ToolBoxPro.ViewModels;
using System.Windows.Controls;

namespace ToolBoxPro.Views
{
    public partial class FileRenameView : UserControl
    {
        public FileRenameView(FileRenameViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
