using ToolBoxPro.ViewModels;

namespace ToolBoxPro.Views
{
    public partial class FileRenamePage : ContentPage
    {
        public FileRenamePage(FileRenameViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
