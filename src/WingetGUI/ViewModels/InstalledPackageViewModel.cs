using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WingetGUI.Core.Contracts.Services;
using WingetGUI.Core.Models;
using Microsoft.Management.Deployment;
using System;

namespace WingetGUI.ViewModels;
public class InstalledPackageViewModel : ObservableObject
{
    private readonly InstalledPackage _model;
    private readonly IPackageManagerService _packageManagerService;
    private readonly IDispatcherService _dispatcherService;
    private bool _selected;
    private string _progressLabel;
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
        set => this.SetProperty(ref _selected, value);
    }

    public string Id => _model.Id;
    public string Name => _model.Name;
    public string Version => _model.Version;
    public string Publisher => _model.Publisher;
    public string NextVersion => _model.NextVersion;
    public AsyncRelayCommand UpgradeCommand => new(() => this.Upgrade(), () => !this.Version.Equals(this.NextVersion));

    public bool Installing
    {
        get => _installing;
        set => this.SetProperty(ref _installing, value);
    }

    public string ProgressLabel
    {
        get => _progressLabel;
        set => this.SetProperty(ref _progressLabel, value);
    }

    public double ProgressPercentage
    {
        get => _progressPercentage;
        set => this.SetProperty(ref _progressPercentage, value);
    }

    private async Task Upgrade()
    {
        _dispatcherService.TryEnqueue(() => this.Installing = true);

        var result = await _packageManagerService.Upgrade(_model, (progress) =>
        {
            _dispatcherService.TryEnqueue(() =>
            {
                switch (progress.State)
                {
                    case PackageInstallProgressState.Queued:
                        this.ProgressLabel = "Queued";
                        break;
                    case PackageInstallProgressState.Downloading:
                        this.ProgressLabel = "Downloading";
                        this.ProgressPercentage = progress.DownloadProgress * 100;
                        break;
                    case PackageInstallProgressState.Installing:
                        this.ProgressLabel = "Installing";
                        this.ProgressPercentage = progress.InstallationProgress * 100;
                        break;
                    case PackageInstallProgressState.PostInstall:
                        this.ProgressLabel = "Post Install";
                        break;
                    case PackageInstallProgressState.Finished:
                        this.ProgressLabel = "Finished";
                        break;
                }
            });
        });

        _dispatcherService.TryEnqueue(() =>
        {
            switch (result)
            {
                case InstallResultStatus.Ok:
                    this.ProgressLabel = "Installed Successfully";
                    this.ProgressPercentage = 100;
                    break;
                case InstallResultStatus.BlockedByPolicy:
                    this.ProgressLabel = "Blocked by Policy";
                    break;
                case InstallResultStatus.CatalogError:
                    this.ProgressLabel = "Catalog Error";
                    break;
                case InstallResultStatus.InternalError:
                    this.ProgressLabel = "Install Error";
                    break;
                case InstallResultStatus.InvalidOptions:
                    this.ProgressLabel = "Invalid Options";
                    break;
                case InstallResultStatus.DownloadError:
                    this.ProgressLabel = "Download Error";
                    break;
                case InstallResultStatus.InstallError:
                    this.ProgressLabel = "Install Error";
                    break;
                case InstallResultStatus.ManifestError:
                    this.ProgressLabel = "Manifest Error";
                    break;
                case InstallResultStatus.NoApplicableInstallers:
                    this.ProgressLabel = "No Applicable Installers";
                    break;
                case InstallResultStatus.NoApplicableUpgrade:
                    this.ProgressLabel = "No Applicable Upgrade";
                    break;
            }

            //this.Installing = false;
        });
    }
}
