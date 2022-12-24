using Microsoft.Management.Deployment;
using WingetGUI.Core.Models;

namespace WingetGUI.Core.Contracts.Services;

public interface IPackageManagerService
{
    Task<IReadOnlyCollection<InstalledPackage>> GetInstalledPackages(string packageCatalogName, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<InstalledPackage>> GetUpgradablePackages(string packageCatalogName, CancellationToken cancellationToken);

    IReadOnlyList<string> GetPackageCatalogs();

    Task<InstallResultStatus> Upgrade(InstalledPackage package, Action<InstallProgress> callback);

    Task<IReadOnlyCollection<SearchResultPackage>> Search(string packageCatalogName, string query, CancellationToken cancellationToken);

    Task<InstallResultStatus> Install(SearchResultPackage package, Action<InstallProgress> callback);
}
