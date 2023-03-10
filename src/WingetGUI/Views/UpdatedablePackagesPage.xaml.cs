using Microsoft.UI.Xaml.Controls;

using WingetGUI.ViewModels;

namespace WingetGUI.Views;

// TODO: Change the grid as appropriate for your app. Adjust the column definitions on DataGridPage.xaml.
// For more details, see the documentation at https://docs.microsoft.com/windows/communitytoolkit/controls/datagrid.
public sealed partial class UpdatedablePackagesPage : Page
{
    public UpdatedablePackagesViewModel ViewModel
    {
        get;
    }

    public UpdatedablePackagesPage()
    {
        ViewModel = App.GetService<UpdatedablePackagesViewModel>();
        InitializeComponent();
    }
}
