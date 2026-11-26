using CommunityToolkit.Maui;
using Microsoft.Maui.Controls.Hosting;
using ToolBoxPro.Core.Interfaces;
using ToolBoxPro.Infrastructure.Services;
using ToolBoxPro.Services;
using ToolBoxPro.ViewModels;
using ToolBoxPro.Views;

namespace ToolBoxPro.Maui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<IDialogService, DialogService>();
            builder.Services.AddSingleton<INavigationService, MauiNavigationService>();
            builder.Services.AddSingleton<IPdfService, PdfProcessingService>();
            builder.Services.AddSingleton<IImageService, ImageProcessingService>();
            builder.Services.AddSingleton<IFileService, FileProcessingService>();
            builder.Services.AddSingleton<IThreeDMapService, ThreeDMapService>();

            builder.Services.AddSingleton<HomeViewModel>();
            builder.Services.AddSingleton<PdfSplitViewModel>();
            builder.Services.AddSingleton<ImageConverterViewModel>();
            builder.Services.AddSingleton<FileRenameViewModel>();
            builder.Services.AddSingleton<ThreeDMapViewModel>();

            builder.Services.AddSingleton<HomePage>();
            builder.Services.AddSingleton<PdfSplitPage>();
            builder.Services.AddSingleton<ImageConverterPage>();
            builder.Services.AddSingleton<FileRenamePage>();
            builder.Services.AddSingleton<ThreeDMapPage>();

            return builder.Build();
        }

        internal static MauiAppBuilder CreateMauiAppBuilder() =>
            MauiApp.CreateBuilder()
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
    }
}
