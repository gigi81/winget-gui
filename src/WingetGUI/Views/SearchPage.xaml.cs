using Microsoft.UI.Xaml.Controls;
using WingetGUI.ViewModels;

namespace WingetGUI.Views;

public sealed partial class SearchPage : Page
{
    public SearchPageViewModel ViewModel { get; }

    public SearchPage()
    {
        ViewModel = App.GetService<SearchPageViewModel>();
        InitializeComponent();
    }
}
