using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Exceptions;
using Artemis.UI.Screens.Plugins;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Builders;

namespace Artemis.UI.Services;

public class PluginInteractionService : IPluginInteractionService
{
    private readonly ICoreService _coreService;
    private readonly IPluginManagementService _pluginManagementService;
    private readonly IWindowService _windowService;
    private readonly INotificationService _notificationService;

    public PluginInteractionService(ICoreService coreService, IPluginManagementService pluginManagementService, IWindowService windowService, INotificationService notificationService)
    {
        _coreService = coreService;
        _pluginManagementService = pluginManagementService;
        _windowService = windowService;
        _notificationService = notificationService;
    }

    /// <inheritdoc />
    public async Task<bool> EnablePlugin(Plugin plugin, bool showMandatoryConfigWindow)
    {
        try
        {
            if (plugin.Info.RequiresAdmin && !_coreService.IsElevated)
            {
                bool confirmed = await _windowService.ShowConfirmContentDialog(
                    "Enable plugin",
                    "This plugin requires admin rights, are you sure you want to enable it? Artemis will need to restart.",
                    "Confirm and restart"
                );
                if (!confirmed)
                    return false;
            }

            // Check if all prerequisites are met async
            List<IPrerequisitesSubject> subjects = [plugin.Info];
            subjects.AddRange(plugin.Features.Where(f => f.AlwaysEnabled || f.EnabledInStorage));

            if (subjects.Any(s => !s.ArePrerequisitesMet()))
            {
                await PluginPrerequisitesInstallDialogViewModel.Show(_windowService, subjects);
                if (!subjects.All(s => s.ArePrerequisitesMet()))
                    return false;
            }

            await Task.Run(() => _pluginManagementService.EnablePlugin(plugin, true, true));
            
            // If the plugin has a mandatory settings window, open it and wait
            if (showMandatoryConfigWindow && plugin.ConfigurationDialog != null && plugin.ConfigurationDialog.IsMandatory)
            {
                if (plugin.Resolve(plugin.ConfigurationDialog.Type) is not PluginConfigurationViewModel viewModel)
                    throw new ArtemisUIException($"The type of a plugin configuration dialog must inherit {nameof(PluginConfigurationViewModel)}");

                await _windowService.ShowDialogAsync(new PluginSettingsWindowViewModel(viewModel));
            }
            
            return true;
        }
        catch (Exception e)
        {
            await ShowPluginToggleFailure(plugin, true, e);
        }

        return false;
    }

    /// <inheritdoc />
    public async Task<bool> DisablePlugin(Plugin plugin)
    {
        try
        {
            await Task.Run(() => _pluginManagementService.DisablePlugin(plugin, true));
            return true;
        }
        catch (Exception e)
        {
            await ShowPluginToggleFailure(plugin, false, e);
        }

        return false;
    }

    /// <inheritdoc />
    public async Task<bool> RemovePlugin(Plugin plugin)
    {
        bool confirmed = await _windowService.ShowConfirmContentDialog("Remove plugin", "Are you sure you want to remove this plugin?");
        if (!confirmed)
            return false;

        // If the plugin or any of its features has uninstall actions, offer to run these
        await RemovePrerequisites(plugin, true);

        try
        {
            _pluginManagementService.RemovePlugin(plugin, false);
        }
        catch (Exception e)
        {
            _windowService.ShowExceptionDialog("Failed to remove plugin", e);
            throw;
        }
        
        _notificationService.CreateNotification().WithTitle("Removed plugin.").Show();
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> RemovePluginSettings(Plugin plugin)
    {
        bool confirmed = await _windowService.ShowConfirmContentDialog("Clear plugin settings", "Are you sure you want to clear the settings of this plugin?");
        if (!confirmed)
            return false;

        bool wasEnabled = plugin.IsEnabled;

        if (wasEnabled)
            _pluginManagementService.DisablePlugin(plugin, false);

        _pluginManagementService.RemovePluginSettings(plugin);

        if (wasEnabled)
            _pluginManagementService.EnablePlugin(plugin, false);

        _notificationService.CreateNotification().WithTitle("Cleared plugin settings.").Show();
        
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> RemovePluginPrerequisites(Plugin plugin, bool forPluginRemoval)
    {
        await RemovePrerequisites(plugin, false);
        return true;
    }

    private async Task ShowPluginToggleFailure(Plugin plugin, bool enable, Exception e)
    {
        string action = enable ? "enable" : "disable";
        ContentDialogBuilder builder = _windowService.CreateContentDialog()
            .WithTitle($"Failed to {action} plugin {plugin.Info.Name}")
            .WithContent(e.Message)
            .HavingPrimaryButton(b => b.WithText("View logs").WithAction(() => Utilities.OpenFolder(Constants.LogsFolder)));
        // If available, add a secondary button pointing to the support page
        if (plugin.Info.HelpPage != null)
            builder = builder.HavingSecondaryButton(b => b.WithText("Open support page").WithAction(() => Utilities.OpenUrl(plugin.Info.HelpPage.ToString())));

        await builder.ShowAsync();
    }
    
    private async Task RemovePrerequisites(Plugin plugin, bool forPluginRemoval)
    {
        List<IPrerequisitesSubject> subjects = [plugin.Info];
        subjects.AddRange(!forPluginRemoval ? plugin.Features.Where(f => f.AlwaysEnabled) : plugin.Features);

        if (subjects.Any(s => s.PlatformPrerequisites.Any(p => p.UninstallActions.Any())))
            await PluginPrerequisitesUninstallDialogViewModel.Show(_windowService, subjects, forPluginRemoval ? "Skip, remove plugin" : "Cancel");
    }
}