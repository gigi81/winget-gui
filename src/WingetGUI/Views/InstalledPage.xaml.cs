using Microsoft.UI.Xaml.Controls;
using CommunityToolkit.WinUI.UI.Controls;
using WingetGUI.ViewModels;
using WingetGUI.Helpers;

namespace WingetGUI.Views;

public sealed partial class InstalledPage : Page
{
    public InstalledPageViewModel ViewModel { get; }

    public InstalledPage()
    {
        ViewModel = App.GetService<InstalledPageViewModel>();
        InitializeComponent();
    }

    private void DataGrid_Sorting(object sender, DataGridColumnEventArgs e)
    {
        var sorting = (sender as DataGrid)?.GetSortDirection(e.Column);
        if (!String.IsNullOrEmpty(sorting))
            this.ViewModel.Sorting = sorting;
    }

    private void CheckBox_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var check = (sender as CheckBox)?.IsChecked;
        if (!check.HasValue)
            return;

        foreach(var package in this.ViewModel.Source)
            package.Selected = check.Value;
    }
}
