﻿using Microsoft.UI.Xaml;

using WingetGUI.Contracts.Services;
using WingetGUI.ViewModels;

namespace WingetGUI.Activation;

public class DefaultActivationHandler : ActivationHandler<LaunchActivatedEventArgs>
{
    private readonly INavigationService _navigationService;

    public DefaultActivationHandler(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
    {
        // None of the ActivationHandlers has handled the activation.
        return _navigationService.Frame?.Content == null;
    }

    protected async override Task HandleInternalAsync(LaunchActivatedEventArgs args)
    {
        _navigationService.NavigateTo(typeof(InstalledPageViewModel).FullName!, args.Arguments);

        await Task.CompletedTask;
    }
}
