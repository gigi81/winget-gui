using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WingetGUI.Core.Contracts.Services;
using WingetGUI.Core.Models;
using Microsoft.Management.Deployment;

namespace WingetGUI.ViewModels.Items;
public class InstalledPackageViewModel : ObservableObject
{
    private readonly InstalledPackage _model;
    private readonly IPackageManagerService _packageManagerService;
    private readonly IDispatcherService _dispatcherService;
    private bool _selected;
    private string _progressLabel = "";
    private double _progressPercentage;
    private bool _installing;

    internal InstalledPackageViewModel(InstalledPackage model, IPackageManagerService packageManagerService, IDispatcherService dispatcherService)
    {
        _model = model;
        _packageManagerService = packageManagerService;
        _dispatcherService = dispatcherService;
    }

    public bool Selected
    {
        get => _selected;
        set => SetProperty(ref _selected, value);
    }

    public string Id => _model.Id;
    public string Name => _model.Name;
    public string Version => _model.Version;
    public string Publisher => _model.Publisher;
    public string NextVersion => _model.NextVersion;
    public AsyncRelayCommand UpgradeCommand => new(() => Upgrade(), () => !Version.Equals(NextVersion));

    public bool Installing
    {
        get => _installing;
        set => SetProperty(ref _installing, value);
    }

    public string ProgressLabel
    {
        get => _progressLabel;
        set => SetProperty(ref _progressLabel, value);
    }

    public double ProgressPercentage
    {
        get => _progressPercentage;
        set => SetProperty(ref _progressPercentage, value);
    }

    private async Task Upgrade()
    {
        _dispatcherService.TryEnqueue(() => Installing = true);

        var result = await _packageManagerService.Upgrade(_model, (progress) =>
        {
            _dispatcherService.TryEnqueue(() =>
            {
                switch (progress.State)
                {
                    case PackageInstallProgressState.Queued:
                        ProgressLabel = "Queued";
                        break;
                    case PackageInstallProgressState.Downloading:
                        ProgressLabel = "Downloading";
                        ProgressPercentage = progress.DownloadProgress * 100;
                        break;
                    case PackageInstallProgressState.Installing:
                        ProgressLabel = "Installing";
                        ProgressPercentage = progress.InstallationProgress * 100;
                        break;
                    case PackageInstallProgressState.PostInstall:
                        ProgressLabel = "Post Install";
                        break;
                    case PackageInstallProgressState.Finished:
                        ProgressLabel = "Finished";
                        break;
                }
            });
        });

        _dispatcherService.TryEnqueue(() =>
        {
            switch (result)
            {
                case InstallResultStatus.Ok:
                    ProgressLabel = "Installed Successfully";
                    ProgressPercentage = 100;
                    break;
                case InstallResultStatus.BlockedByPolicy:
                    ProgressLabel = "Blocked by Policy";
                    break;
                case InstallResultStatus.CatalogError:
                    ProgressLabel = "Catalog Error";
                    break;
                case InstallResultStatus.InternalError:
                    ProgressLabel = "Install Error";
                    break;
                case InstallResultStatus.InvalidOptions:
                    ProgressLabel = "Invalid Options";
                    break;
                case InstallResultStatus.DownloadError:
                    ProgressLabel = "Download Error";
                    break;
                case InstallResultStatus.InstallError:
                    ProgressLabel = "Install Error";
                    break;
                case InstallResultStatus.ManifestError:
                    ProgressLabel = "Manifest Error";
                    break;
                case InstallResultStatus.NoApplicableInstallers:
                    ProgressLabel = "No Applicable Installers";
                    break;
                case InstallResultStatus.NoApplicableUpgrade:
                    ProgressLabel = "No Applicable Upgrade";
                    break;
            }

            //this.Installing = false;
        });
    }
}
