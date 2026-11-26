namespace ToolBoxPro.Core.Interfaces
{
    public interface INavigationService
    {
        void NavigateTo(string viewName);
        void NavigateTo(string viewName, object? parameter);
        Task NavigateToAsync(string viewName);
        Task NavigateToAsync(string viewName, object? parameter);
        bool CanNavigateBack { get; }
        void NavigateBack();
        event EventHandler<ToolBoxNavigationEventArgs>? Navigated;
    }

    public class ToolBoxNavigationEventArgs : EventArgs
    {
        public string ViewName { get; }
        public object? Parameter { get; }

        public ToolBoxNavigationEventArgs(string viewName, object? parameter)
        {
            ViewName = viewName;
            Parameter = parameter;
        }
    }
}
