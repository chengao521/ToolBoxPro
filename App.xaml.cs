using Microsoft.Extensions.DependencyInjection;
using ToolBoxPro.Core.Interfaces;
using ToolBoxPro.Infrastructure.Services;
using ToolBoxPro.Services;
using ToolBoxPro.ViewModels;
using ToolBoxPro.Views;
using System.Windows;

namespace ToolBoxPro
{
    public partial class App : Application
    {
        private ServiceProvider? _serviceProvider;
        private MainWindow? _mainWindow;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            _mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            _mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            _serviceProvider = null;
            _mainWindow = null;
            base.OnExit(e);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // PDF Services
            services.AddSingleton<IPdfService, PdfProcessingService>();
            
            // Common Services
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IImageService, ImageProcessingService>();
            services.AddSingleton<IFileService, FileProcessingService>();
            services.AddSingleton<IThreeDMapService, ThreeDMapService>();
            services.AddSingleton<IProteinViewerService, ProteinViewerService>();

            // ViewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<HomeViewModel>();
            services.AddTransient<PdfSplitViewModel>();
            services.AddTransient<ImageConverterViewModel>();
            services.AddTransient<FileRenameViewModel>();
            services.AddTransient<ScrollCaptureViewModel>();
            services.AddTransient<ThreeDMapViewModel>();
            services.AddTransient<ProteinViewerViewModel>();

            // Views
            services.AddTransient<MainWindow>();
            services.AddTransient<HomeView>();
            services.AddTransient<PdfSplitView>();
            services.AddTransient<ImageConverterView>();
            services.AddTransient<FileRenameView>();
            services.AddTransient<ScrollCaptureView>();
            services.AddTransient<ThreeDMapView>();
            services.AddTransient<ProteinViewerView>();
        }
    }
}
