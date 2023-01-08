using Microsoft.UI.Xaml.Controls;
using WingetGUI.ViewModels;

namespace WingetGUI.Views;

public sealed partial class SearchPackagesPage : Page
{
    public SearchPageViewModel ViewModel { get; }

    public SearchPackagesPage()
    {
        ViewModel = App.GetService<SearchPageViewModel>();
        InitializeComponent();
    }
}
