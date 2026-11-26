using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ToolBoxPro.Core.Interfaces;

namespace ToolBoxPro.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private string _title = "ToolBox Pro - 多功能工具箱";

        [ObservableProperty]
        private bool _isPaneOpen = true;

        public MainViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        [RelayCommand]
        private void TogglePane()
        {
            IsPaneOpen = !IsPaneOpen;
        }

        [RelayCommand]
        private void NavigateTo(string viewName)
        {
            _navigationService.NavigateTo(viewName);
        }
    }
}
