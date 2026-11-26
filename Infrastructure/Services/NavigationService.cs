using Microsoft.Extensions.DependencyInjection;
using ToolBoxPro.Core.Interfaces;
using System.Windows.Controls;

namespace ToolBoxPro.Infrastructure.Services
{
    public class NavigationService : INavigationService
    {
        private readonly Dictionary<string, Type> _views = new();
        private readonly Stack<string> _navigationStack = new();
        private Frame? _frame;
        private readonly IServiceProvider _serviceProvider;

        public event EventHandler<NavigationEventArgs>? Navigated;

        public bool CanNavigateBack => _navigationStack.Count > 1;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Initialize(Frame frame)
        {
            _frame = frame;
        }

        public void RegisterView(string viewName, Type viewType)
        {
            if (!_views.ContainsKey(viewName))
            {
                _views.Add(viewName, viewType);
            }
        }

        public void NavigateTo(string viewName)
        {
            NavigateTo(viewName, null);
        }

        public void NavigateTo(string viewName, object? parameter)
        {
            if (_frame == null) throw new InvalidOperationException("NavigationService未初始化");
            if (!_views.TryGetValue(viewName, out var viewType)) throw new ArgumentException($"未找到视图: {viewName}");

            var view = _serviceProvider.GetRequiredService(viewType);
            _frame.Navigate(view);

            _navigationStack.Push(viewName);
            Navigated?.Invoke(this, new NavigationEventArgs(viewName, parameter));
        }

        public void NavigateBack()
        {
            if (_frame == null || !CanNavigateBack) return;

            _navigationStack.Pop();
            var previousView = _navigationStack.Peek();

            if (_views.TryGetValue(previousView, out var viewType))
            {
                var view = _serviceProvider.GetRequiredService(viewType);
                _frame.GoBack();
            }
        }
    }
}
