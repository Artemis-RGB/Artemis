﻿using Artemis.Core.Services;
using Artemis.UI.Screens.StartupWizard;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Avalonia.Threading;

namespace Artemis.UI.Screens.Home;

public class HomeViewModel : ViewModelBase, IMainScreenViewModel
{
    public HomeViewModel(ISettingsService settingsService, IWindowService windowService)
    {
        // Show the startup wizard if it hasn't been completed
        if (!settingsService.GetSetting("UI.SetupWizardCompleted", false).Value)
            Dispatcher.UIThread.InvokeAsync(async () => await windowService.ShowDialogAsync<StartupWizardViewModel, bool>());
    }

    public ViewModelBase? TitleBarViewModel => null;
}