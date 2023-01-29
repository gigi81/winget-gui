using Microsoft.Extensions.Logging;
using Microsoft.Management.Deployment;
using Microsoft.Management.Deployment.Projection;
using WingetGUI.Core.Contracts.Services;
using WingetGUI.Core.Models;

namespace WingetGUI.Core.Services;
public class PackageManagerService : IPackageManagerService
{
    private static readonly Lazy<WinGetProjectionFactory> WingetProjectionFactory = new(CreateFactory);
    private static readonly Lazy<PackageManager> PackageManager = new(() => WingetProjectionFactory.Value.CreatePackageManager());
    private static readonly Lazy<IReadOnlyList<PackageCatalogReference>> PackageCatalogs = new(() => PackageManager.Value.GetPackageCatalogs());
    private static readonly Dictionary<string, PackageCatalog> _localCatalogues = new();
    private static readonly Dictionary<string, PackageCatalog> _remoteCatalogues = new();
    private readonly ILogger<PackageManagerService> _logger;

    public PackageManagerService(ILogger<PackageManagerService> logger)
    {
        _logger = logger;
    }

    public IReadOnlyList<string> GetPackageCatalogs() => PackageCatalogs.Value.Select(c => c.Info.Name).ToArray();

    public async Task<IReadOnlyCollection<InstalledPackage>> GetInstalledPackages(string packageCatalogName, CancellationToken cancellationToken)
    {
        try
        {
            var matches = await FindLocalPackages(packageCatalogName);
            var ret = new List<InstalledPackage>();

            foreach (var match in matches)
            {
                var package = match.CatalogPackage;
                if (package.InstalledVersion == null)
                    continue;

                ret.Add(new InstalledPackage
                {
                    Id = package.Id,
                    Name = package.Name,
                    Publisher = package.InstalledVersion.Publisher,
                    Version = package.InstalledVersion.Version,
                    NextVersion = package?.DefaultInstallVersion?.Version
                });
            }

            return ret;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve list of installed packages");
            throw;
        }
    }

    private static WinGetProjectionFactory CreateFactory()
    {
        var init = new LocalServerInstanceInitializer { AllowLowerTrustRegistration = true, UseDevClsids = false };
        //var init = new ActivationFactoryInstanceInitializer();
        return new WinGetProjectionFactory(init);
    }

    public async Task<IReadOnlyCollection<InstalledPackage>> GetUpgradablePackages(string packageCatalogName, CancellationToken cancellationToken)
    {
        try
        {
            var matches = await FindLocalPackages(packageCatalogName);
            var ret = new List<InstalledPackage>();

            foreach (var match in matches)
            {
                var package = match.CatalogPackage;
                if (package.InstalledVersion == null)
                    continue;

                if (!package.IsUpdateAvailable)
                    continue;

                ret.Add(new InstalledPackage(package));
            }

            return ret;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve list of upgradable packages");
            throw;
        }
    }

    public async Task<IReadOnlyCollection<SearchResultPackage>> Search(string packageCatalogName, string query, CancellationToken cancellationToken)
    {
        try
        {
            var catalog = await GetRemotePackageCatalog(packageCatalogName);
            var options = WingetProjectionFactory.Value.CreateFindPackagesOptions();
            var filter = WingetProjectionFactory.Value.CreatePackageMatchFilter();
            filter.Field = PackageMatchField.Id;
            filter.Option = PackageFieldMatchOption.ContainsCaseInsensitive;
            filter.Value = query;
            options.Filters.Add(filter);
            var packages = await catalog.FindPackagesAsync(options);
            var matches = packages.Matches.ToArray();

            var ret = new List<SearchResultPackage>();

            foreach (var match in matches)
                ret.Add(new SearchResultPackage(match.CatalogPackage));

            return ret;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve list of upgradable packages");
            throw;
        }
    }

    public Task<InstallResultStatus> Upgrade(InstalledPackage package, Action<InstallProgress> callback)
    {
        return InstallPackage(package._package, PackageInstallScope.Any, callback);
    }

    public Task<InstallResultStatus> Install(SearchResultPackage package, InstallScope scope, Action<InstallProgress> callback)
    {
        return InstallPackage(package._package, MapScope(scope), callback);
    }

    private async Task<InstallResultStatus> InstallPackage(CatalogPackage package, PackageInstallScope scope, Action<InstallProgress> callback)
    {
        var installOptions = WingetProjectionFactory.Value.CreateInstallOptions();
        installOptions.PackageInstallMode = PackageInstallMode.Silent;
        installOptions.PackageInstallScope = scope;
        var installOperation = PackageManager.Value.InstallPackageAsync(package, installOptions);

        installOperation.Progress = (_, progress) =>
        {
            callback?.Invoke(progress);
        };

        return (await installOperation).Status;
    }

    private static async Task<MatchResult[]> FindLocalPackages(string packageCatalogName)
    {
        var catalog = await GetLocalPackageCatalog(packageCatalogName);
        var options = WingetProjectionFactory.Value.CreateFindPackagesOptions();
        var packages = await catalog.FindPackagesAsync(options);
        var matches = packages.Matches.ToArray();
        return matches;
    }

    private static async Task<PackageCatalog> GetLocalPackageCatalog(string packageCatalogName)
    {
        if (_localCatalogues.TryGetValue(packageCatalogName, out var ret))
            return ret;

        var options = WingetProjectionFactory.Value.CreateCreateCompositePackageCatalogOptions();
        options.Catalogs.Add(PackageManager.Value.GetPackageCatalogByName(packageCatalogName));
        options.CompositeSearchBehavior = CompositeSearchBehavior.LocalCatalogs;
        var catalogReference = PackageManager.Value.CreateCompositePackageCatalog(options);
        var connectResult = await catalogReference.ConnectAsync();

        ret = connectResult.PackageCatalog;
        _localCatalogues.Add(packageCatalogName, ret);
        return ret;
    }

    private static async Task<PackageCatalog> GetRemotePackageCatalog(string packageCatalogName)
    {
        var options = WingetProjectionFactory.Value.CreateCreateCompositePackageCatalogOptions();
        options.Catalogs.Add(PackageManager.Value.GetPackageCatalogByName(packageCatalogName));
        var catalogReference = PackageManager.Value.CreateCompositePackageCatalog(options);
        var connectResult = await catalogReference.ConnectAsync();

        return connectResult.PackageCatalog;
    }

    private PackageInstallScope MapScope(InstallScope scope)
    {
        switch (scope)
        {
            case InstallScope.Any: return PackageInstallScope.Any;
            case InstallScope.User: return PackageInstallScope.User;
            case InstallScope.System: return PackageInstallScope.System;
        }

        throw new ArgumentException(nameof(scope));
    }
}
