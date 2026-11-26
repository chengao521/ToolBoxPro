using System.Windows.Controls;

namespace ToolBoxPro.Core.Interfaces
{
    public interface INavigationService
    {
        void NavigateTo(string viewName);
        void NavigateTo(string viewName, object? parameter);
        bool CanNavigateBack { get; }
        void NavigateBack();
        void Initialize(Frame frame);
        void RegisterView(string viewName, Type viewType);
        event EventHandler<NavigationEventArgs>? Navigated;
    }

    public class NavigationEventArgs : EventArgs
    {
        public string ViewName { get; }
        public object? Parameter { get; }

        public NavigationEventArgs(string viewName, object? parameter)
        {
            ViewName = viewName;
            Parameter = parameter;
        }
    }
}
