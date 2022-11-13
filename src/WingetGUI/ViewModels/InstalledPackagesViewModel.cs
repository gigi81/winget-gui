using System.Collections.ObjectModel;
using System.Linq.Dynamic.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using WingetGUI.Contracts.ViewModels;
using WingetGUI.Core.Contracts.Services;
using WingetGUI.Core.Models;
using WingetGUI.Services;

namespace WingetGUI.ViewModels;

public class InstalledPackagesViewModel : ObservableRecipient, INavigationAware
{
    private readonly IPackageManagerService _packageManagerService;
    private readonly IDispatcherService _dispatcherService;
    private string _sorting = nameof(InstalledPackage.Name);
    private string? _packageCatalogName;
    private bool _loading;
    private IList<InstalledPackage> _source = new List<InstalledPackage>();

    public IList<InstalledPackage> Source
    {
        get => _source;
        private set => this.SetProperty(ref _source, value);
    }

    public ObservableCollection<string> Catalogues { get; } = new ObservableCollection<string>();

    public InstalledPackagesViewModel(IPackageManagerService packageManagerService)
    {
        _packageManagerService = packageManagerService;
        _dispatcherService = DispatcherService.FromCurrentThread();
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

        this.Catalogues.Clear();
        foreach (var catalogue in _packageManagerService.GetPackageCatalogs())
            this.Catalogues.Add(catalogue);

        this.PackageCatalogName = this.Catalogues.FirstOrDefault(n => n.Contains("winget", StringComparison.CurrentCultureIgnoreCase)) ??
                                  this.Catalogues.FirstOrDefault();
    }

    private async Task UpdateSource()
    {
        try
        {
            _dispatcherService.TryEnqueue(() => this.Loading = true);
            //_dispatcherService.TryEnqueue(() => Source.Clear());

            if (String.IsNullOrEmpty(this.PackageCatalogName))
                return;

            var packages = await _packageManagerService.GetInstalledPackages(this.PackageCatalogName, new CancellationToken());

            _dispatcherService.TryEnqueue(() =>
            {
                this.UpdateSource(packages);
            });
        }
        finally
        {
            _dispatcherService.TryEnqueue(() => this.Loading = false);
        }
    }

    private void UpdateSource(IEnumerable<InstalledPackage> packages)
    {
        var sorted = packages.AsQueryable().OrderBy(this.Sorting).ToArray();
        this.Source = sorted.ToList();
    }

    public void OnNavigatedFrom()
    {
    }
}
