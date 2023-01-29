using CommunityToolkit.WinUI.UI.Controls;

namespace WingetGUI.Helpers;
internal static class SortingExtensions
{
    public static string? GetSortDirection(this DataGrid grid, DataGridColumn column)
    {
        var tag = column.Tag as string;
        if (grid == null || String.IsNullOrEmpty(tag))
            return null;

        //reset previous sorting
        foreach (var c in grid.Columns.Where(c => c != column))
            c.SortDirection = null;

        if (column.SortDirection == null || column.SortDirection == DataGridSortDirection.Descending)
        {
            column.SortDirection = DataGridSortDirection.Ascending;
            return $"{tag} asc";
        }

        column.SortDirection = DataGridSortDirection.Descending;
        return $"{tag} desc";
    }
}
