using Microsoft.Extensions.DependencyInjection;
using ToolBoxPro.Core.Interfaces;
using ToolBoxPro.Views;

namespace ToolBoxPro.Infrastructure.Services
{
    public class MauiNavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Stack<string> _navigationStack = new();

        public event EventHandler<ToolBoxNavigationEventArgs>? Navigated;
        public bool CanNavigateBack => _navigationStack.Count > 1;

        public MauiNavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task NavigateToAsync(string viewName)
        {
            var pageType = GetPageType(viewName);
            if (pageType == null)
                throw new ArgumentException($"未找到页面: {viewName}");

            var page = (Page)_serviceProvider.GetRequiredService(pageType);
            await Application.Current.MainPage.Navigation.PushAsync(page);

            _navigationStack.Push(viewName);
            Navigated?.Invoke(this, new ToolBoxNavigationEventArgs(viewName, null));
        }

        public void NavigateTo(string viewName)
        {
            _ = NavigateToAsync(viewName);
        }

        public void NavigateTo(string viewName, object? parameter)
        {
            _ = NavigateToAsync(viewName, parameter);
        }

        public async Task NavigateToAsync(string viewName, object? parameter)
        {
            var pageType = GetPageType(viewName);
            if (pageType == null)
                throw new ArgumentException($"未找到页面: {viewName}");

            var page = (Page)_serviceProvider.GetRequiredService(pageType);
            await Application.Current.MainPage.Navigation.PushAsync(page);

            _navigationStack.Push(viewName);
            Navigated?.Invoke(this, new ToolBoxNavigationEventArgs(viewName, parameter));
        }

        public async Task NavigateBackAsync()
        {
            if (_navigationStack.Count <= 1) return;

            _navigationStack.Pop();
            await Application.Current.MainPage.Navigation.PopAsync();
        }

        public void NavigateBack()
        {
            _ = NavigateBackAsync();
        }

        private Type? GetPageType(string viewName)
        {
            return viewName switch
            {
                "PdfSplitPage" => typeof(PdfSplitPage),
                "ImageConverterPage" => typeof(ImageConverterPage),
                "FileRenamePage" => typeof(FileRenamePage),
                "ThreeDMapPage" => typeof(ThreeDMapPage),
                _ => null
            };
        }
    }
}
