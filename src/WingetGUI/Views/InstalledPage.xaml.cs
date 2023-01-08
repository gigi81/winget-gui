using Microsoft.UI.Xaml.Controls;
using CommunityToolkit.WinUI.UI.Controls;
using WingetGUI.ViewModels;

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
        var grid = sender as DataGrid;
        var column = e.Column.Tag as string;
        if (grid == null || String.IsNullOrEmpty(column))
            return;

        this.ViewModel.Sorting = $"{column} {GetSortDirection(grid, e)}";
    }

    private string GetSortDirection(DataGrid grid, DataGridColumnEventArgs e)
    {
        //reset previous sorting
        foreach (var column in grid.Columns.Where(c => c != e.Column))
            column.SortDirection = null;

        if (e.Column.SortDirection == null || e.Column.SortDirection == DataGridSortDirection.Descending)
        {
            e.Column.SortDirection = DataGridSortDirection.Ascending;
            return "asc";
        }

        e.Column.SortDirection = DataGridSortDirection.Descending;
        return "desc";
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
