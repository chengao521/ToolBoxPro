using ToolBoxPro.Core.Interfaces;
using ToolBoxPro.Infrastructure.Services;
using ToolBoxPro.ViewModels;
using ToolBoxPro.Views;
using System.Windows;

namespace ToolBoxPro
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel, INavigationService navigationService)
        {
            InitializeComponent();
            DataContext = viewModel;

            var navService = (NavigationService)navigationService;
            navService.Initialize(MainFrame);
            navService.RegisterView("Home", typeof(HomeView));
            navService.RegisterView("PdfSplit", typeof(PdfSplitView));
            navService.RegisterView("ImageConverter", typeof(ImageConverterView));
            navService.RegisterView("FileRename", typeof(FileRenameView));
            navService.RegisterView("ScrollCapture", typeof(ScrollCaptureView));
            navService.RegisterView("ThreeDMap", typeof(ThreeDMapView));
            navService.RegisterView("ProteinViewer", typeof(ProteinViewerView));

            navigationService.NavigateTo("Home");
        }
    }
}
