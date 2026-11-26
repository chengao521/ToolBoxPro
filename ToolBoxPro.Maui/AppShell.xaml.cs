using ToolBoxPro.ViewModels;
using ToolBoxPro.Views;

namespace ToolBoxPro.Maui
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
#if WINDOWS
            Routing.RegisterRoute("HomePage", typeof(HomePage));
            Routing.RegisterRoute("PdfSplitPage", typeof(PdfSplitPage));
            Routing.RegisterRoute("ImageConverterPage", typeof(ImageConverterPage));
            Routing.RegisterRoute("FileRenamePage", typeof(FileRenamePage));
            Routing.RegisterRoute("ThreeDMapPage", typeof(ThreeDMapPage));
#endif
        }
    }
}
