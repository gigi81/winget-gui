using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml.Controls;
using WingetGUI.Helpers;
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

    private void DataGrid_Sorting(object sender, CommunityToolkit.WinUI.UI.Controls.DataGridColumnEventArgs e)
    {
        var sorting = (sender as DataGrid)?.GetSortDirection(e.Column);
        if (!String.IsNullOrEmpty(sorting))
            this.ViewModel.Sorting = sorting;
    }
}
