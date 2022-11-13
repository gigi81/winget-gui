using Microsoft.UI.Xaml.Controls;
using CommunityToolkit.WinUI.UI.Controls;

using WingetGUI.ViewModels;

namespace WingetGUI.Views;

// TODO: Change the grid as appropriate for your app. Adjust the column definitions on DataGridPage.xaml.
// For more details, see the documentation at https://docs.microsoft.com/windows/communitytoolkit/controls/datagrid.
public sealed partial class InstalledPackagesPage : Page
{
    public InstalledPackagesViewModel ViewModel
    {
        get;
    }

    public InstalledPackagesPage()
    {
        ViewModel = App.GetService<InstalledPackagesViewModel>();
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
        foreach (var column in grid.Columns)
        {
            if (column != e.Column)
                column.SortDirection = null;
        }

        if (e.Column.SortDirection == null || e.Column.SortDirection == DataGridSortDirection.Descending)
        {
            e.Column.SortDirection = DataGridSortDirection.Ascending;
            return "asc";
        }

        e.Column.SortDirection = DataGridSortDirection.Descending;
        return "desc";
    }
}
