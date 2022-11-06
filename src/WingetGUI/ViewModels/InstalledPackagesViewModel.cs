using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;

using WingetGUI.Contracts.ViewModels;
using WingetGUI.Core.Contracts.Services;
using WingetGUI.Core.Models;

namespace WingetGUI.ViewModels;

public class InstalledPackagesViewModel : ObservableRecipient, INavigationAware
{
    private readonly ISampleDataService _sampleDataService;

    public ObservableCollection<SampleOrder> Source { get; } = new ObservableCollection<SampleOrder>();

    public InstalledPackagesViewModel(ISampleDataService sampleDataService)
    {
        _sampleDataService = sampleDataService;
    }

    public async void OnNavigatedTo(object parameter)
    {
        Source.Clear();

        // TODO: Replace with real data.
        var data = await _sampleDataService.GetGridDataAsync();

        foreach (var item in data)
        {
            Source.Add(item);
        }
    }

    public void OnNavigatedFrom()
    {
    }
}
