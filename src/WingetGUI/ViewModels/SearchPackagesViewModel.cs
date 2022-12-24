using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;

using WingetGUI.Contracts.ViewModels;
using WingetGUI.Core.Contracts.Services;
using WingetGUI.Core.Models;
using WingetGUI.Services;

namespace WingetGUI.ViewModels;

public class SearchPackagesViewModel : ObservableRecipient, INavigationAware
{
    private string _search;
    private bool _searching;
    private readonly IPackageManagerService _packageManagerService;
    private readonly IDispatcherService _dispatcherService;
    private IList<SearchResultPackage> _source = new List<SearchResultPackage>();

    public SearchPackagesViewModel(IPackageManagerService packageManagerService)
    {
        _packageManagerService = packageManagerService;
        _dispatcherService = DispatcherService.FromCurrentThread();
    }

    public IList<SearchResultPackage> Source
    {
        get => _source;
        private set => this.SetProperty(ref _source, value);
    }

    public string Search
    {
        get => _search;
        set => SetProperty(ref _search, value);
    }

    public bool CanSearch => !this.Searching;

    public bool Searching
    {
        get => _searching;
        set
        {
            SetProperty(ref _searching, value);
            OnPropertyChanged(nameof(CanSearch));
        }
    }

    public void SearchSubmitted()
    {
        this.Searching = true;

        Task.Run(async () => {
            var source = await _packageManagerService.Search("winget", this.Search, CancellationToken.None);

            _dispatcherService.TryEnqueue(() =>
            {
                this.Source = source.ToList();
                this.Searching = false;
            });
        });
    }

    public void OnNavigatedTo(object parameter)
    {
    }

    public void OnNavigatedFrom()
    {
    }
}
