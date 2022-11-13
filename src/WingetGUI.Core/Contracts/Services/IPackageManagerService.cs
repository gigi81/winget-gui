using WingetGUI.Core.Models;

namespace WingetGUI.Core.Contracts.Services;

public interface IPackageManagerService
{
    Task<IReadOnlyCollection<InstalledPackage>> GetInstalledPackages(string packageCatalogName, CancellationToken cancellationToken);

    IReadOnlyList<string> GetPackageCatalogs();
}
