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
            var catalogReference = PackageManager.Value.GetLocalPackageCatalog(LocalPackageCatalog.InstalledPackages);
            var connectResult = await catalogReference.ConnectAsync();
            var options = WingetProjectionFactory.Value.CreateFindPackagesOptions();

            var packages = await connectResult.PackageCatalog.FindPackagesAsync(options);
            //we need to materialize here to avoid some odd InvalidInterface exception
            var matches = packages.Matches.ToArray();

            var ret = new List<InstalledPackage>();

            foreach (var match in matches)
            {
                var package = match.CatalogPackage;
                if (!FilterByCatalog(packageCatalogName, package))
                    continue;

                ret.Add(new InstalledPackage
                {
                    Id = package.Id,
                    Name = package.Name,
                    Publisher = package.InstalledVersion.Publisher,
                    Version = package.InstalledVersion.Version
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

    private static bool FilterByCatalog(string packageCatalogName, CatalogPackage package)
    {
        if(String.IsNullOrEmpty(package.InstalledVersion.PackageCatalog.Info.Name))
            return true;

        return true;
    }

    private static WinGetProjectionFactory CreateFactory()
    {
        var init = new LocalServerInstanceInitializer { AllowLowerTrustRegistration = true, UseDevClsids = false };
        //var init = new ActivationFactoryInstanceInitializer();
        return new WinGetProjectionFactory(init);
    }
}
