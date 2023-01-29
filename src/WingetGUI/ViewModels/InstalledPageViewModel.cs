using System.Collections.ObjectModel;
using System.Linq.Dynamic.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WingetGUI.Contracts.ViewModels;
using WingetGUI.Core.Contracts.Services;
using WingetGUI.Core.Models;
using WingetGUI.Services;
using WingetGUI.ViewModels.Items;

namespace WingetGUI.ViewModels;

public class InstalledPageViewModel : ObservableRecipient, INavigationAware
{
    private readonly IPackageManagerService _packageManagerService;
    private readonly IDispatcherService _dispatcherService;
    private string _sorting = nameof(InstalledPackageViewModel.Name);
    private string? _packageCatalogName;
    private bool _loading;
    private IList<InstalledPackageViewModel> _source = new List<InstalledPackageViewModel>();

    public IList<InstalledPackageViewModel> Source
    {
        get => _source;
        private set => this.SetProperty(ref _source, value);
    }

    public ObservableCollection<string> Catalogues { get; } = new ObservableCollection<string>();

    public InstalledPageViewModel(IPackageManagerService packageManagerService)
    {
        _packageManagerService = packageManagerService;
        _dispatcherService = DispatcherService.FromCurrentThread();
    }

    public RelayCommand SelectAllCommand => new(this.SelectAll, () => true);

    private void SelectAll()
    {
        foreach(var package in this.Source)
            package.Selected = true;
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

    public bool Loading
    {
        get => _loading;
        set => this.SetProperty(ref _loading, value);
    }

    public string? PackageCatalogName
    {
        get => _packageCatalogName;
        set
        {
            if (!this.SetProperty(ref _packageCatalogName, value))
                return;

            Task.Run(() => this.UpdateSource());
        }
    }

    public async void OnNavigatedTo(object parameter)
    {
        if (!String.IsNullOrEmpty(this.PackageCatalogName))
            return;

        await Task.Run(() =>
        {
            var catalogues = _packageManagerService.GetPackageCatalogs();

            _dispatcherService.TryEnqueue(() =>
            {
                this.Catalogues.Clear();
                foreach (var catalogue in catalogues)
                    this.Catalogues.Add(catalogue);

                this.PackageCatalogName = this.Catalogues.FirstOrDefault(n => n.Contains("winget", StringComparison.CurrentCultureIgnoreCase)) ??
                                          this.Catalogues.FirstOrDefault();

            });
        });
    }

    private async Task UpdateSource()
    {
        try
        {
            _dispatcherService.TryEnqueue(() =>
            {
                this.Loading = true;
                this.Source.Clear();
            });

            if (String.IsNullOrEmpty(this.PackageCatalogName))
                return;

            var packages = await _packageManagerService.GetUpgradablePackages(this.PackageCatalogName, new CancellationToken());

            _dispatcherService.TryEnqueue(() =>
            {
                this.UpdateSource(packages.Select(CreatePackageViewModel).ToList());
            });
        }
        finally
        {
            _dispatcherService.TryEnqueue(() => this.Loading = false);
        }
    }

    private InstalledPackageViewModel CreatePackageViewModel(InstalledPackage p) => new(p, _packageManagerService, _dispatcherService);

    private void UpdateSource(IEnumerable<InstalledPackageViewModel> packages)
    {
        var sorted = packages.AsQueryable().OrderBy(this.Sorting).ToArray();
        this.Source = sorted.ToList();
    }

    public void OnNavigatedFrom()
    {
    }
}
