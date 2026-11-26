using ToolBoxPro.Views;

namespace ToolBoxPro.Maui
{
    public partial class App : Microsoft.Maui.Controls.Application
    {
        public App()
        {
            MainPage = new AppShell();
        }
    }
}
