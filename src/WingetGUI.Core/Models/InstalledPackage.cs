using Microsoft.Management.Deployment;

namespace WingetGUI.Core.Models;
public class InstalledPackage
{
    internal readonly CatalogPackage _package;

    public InstalledPackage()
    {
    }

    internal InstalledPackage(CatalogPackage package)
    {
        _package = package;

        Id = package.Id;
        Name = package.Name;
        Publisher = package.InstalledVersion.Publisher;
        Version = package.InstalledVersion.Version;
        NextVersion = package?.DefaultInstallVersion?.Version;
    }

    public string Id { get; set; }
    public string Name { get; set; }
    public string Version { get; set; }
    public string Publisher { get; set; }
    public string NextVersion { get; set; }
}
