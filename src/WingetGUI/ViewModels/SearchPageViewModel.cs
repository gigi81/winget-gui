using CommunityToolkit.Mvvm.ComponentModel;
using System.Linq.Dynamic.Core;
using WingetGUI.Contracts.ViewModels;
using WingetGUI.Core.Contracts.Services;
using WingetGUI.Core.Models;
using WingetGUI.Services;
using WingetGUI.ViewModels.Items;

namespace WingetGUI.ViewModels;

public class SearchPageViewModel : ObservableRecipient, INavigationAware
{
    private string _search = "";
    private bool _searching;
    private string _sorting = nameof(SearchPackageViewModel.Name);
    private readonly IPackageManagerService _packageManagerService;
    private readonly IDispatcherService _dispatcherService;
    private IList<SearchPackageViewModel> _source = new List<SearchPackageViewModel>();

    public SearchPageViewModel(IPackageManagerService packageManagerService)
    {
        _packageManagerService = packageManagerService;
        _dispatcherService = DispatcherService.FromCurrentThread();
    }

    public IList<SearchPackageViewModel> Source
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

    public string Sorting
    {
        get => _sorting;
        set
        {
            if (String.IsNullOrWhiteSpace(value))
                return;

            if (SetProperty(ref _sorting, value))
                this.UpdateSource(this.Source);
        }
    }

    public void SearchSubmitted()
    {
        this.Searching = true;

        Task.Run(async () => {
            var source = await _packageManagerService.Search("winget", this.Search, CancellationToken.None);

            _dispatcherService.TryEnqueue(() =>
            {
                this.UpdateSource(source.Select(CreatePackageViewModel).ToList());
                this.Searching = false;
            });
        });
    }

    private SearchPackageViewModel CreatePackageViewModel(SearchResultPackage p) => new(p, _packageManagerService, _dispatcherService);

    private void UpdateSource(IEnumerable<SearchPackageViewModel> packages)
    {
        var sorted = packages.AsQueryable().OrderBy(this.Sorting).ToArray();
        this.Source = sorted.ToList();
    }

    public void OnNavigatedTo(object parameter)
    {
    }

    public void OnNavigatedFrom()
    {
    }
}
