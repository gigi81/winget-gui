using Microsoft.Management.Deployment;

namespace WingetGUI.Core.Models;

public class SearchResultPackage
{
    internal readonly CatalogPackage _package;

    public SearchResultPackage(CatalogPackage package)
    {
        _package = package;

        Id = package.Id;
        Name = package.Name;
        Publisher = package.DefaultInstallVersion?.Publisher;
        Version = package.DefaultInstallVersion?.Version;
        InstalledVersion = package.InstalledVersion?.Version;
    }

    public string Id { get; set; }
    public string Name { get; set; }
    public string Publisher { get; set; }
    public string Version { get; set; }
    public string InstalledVersion { get; set; }
}
