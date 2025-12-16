using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Exceptions;
using Artemis.UI.Screens.Plugins;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Builders;
using Avalonia.Controls;
using Avalonia.Threading;
using Material.Icons;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.StartupWizard;

public partial class WizardPluginFeatureViewModel : ActivatableViewModelBase
{
    private readonly ICoreService _coreService;
    private readonly IPluginManagementService _pluginManagementService;
    private readonly IWindowService _windowService;
    private Window? _settingsWindow;
    [Notify] private bool _canInstallPrerequisites;
    [Notify] private bool _canRemovePrerequisites;
    [Notify] private bool _enabling;

    public WizardPluginFeatureViewModel(PluginFeatureInfo pluginFeature, ICoreService coreService, IWindowService windowService, IPluginManagementService pluginManagementService)
    {
        PluginFeature = pluginFeature;
        Plugin = pluginFeature.Plugin;

        _coreService = coreService;
        _windowService = windowService;
        _pluginManagementService = pluginManagementService;

        Platforms = new ObservableCollection<PluginPlatformViewModel>();
        if (Plugin.Info.Platforms != null)
        {
            if (Plugin.Info.Platforms.Value.HasFlag(PluginPlatform.Windows))
                Platforms.Add(new PluginPlatformViewModel("Windows", MaterialIconKind.MicrosoftWindows));
            if (Plugin.Info.Platforms.Value.HasFlag(PluginPlatform.Linux))
                Platforms.Add(new PluginPlatformViewModel("Linux", MaterialIconKind.Linux));
            if (Plugin.Info.Platforms.Value.HasFlag(PluginPlatform.OSX))
                Platforms.Add(new PluginPlatformViewModel("OSX", MaterialIconKind.Apple));
        }

        OpenSettings = ReactiveCommand.Create(ExecuteOpenSettings, this.WhenAnyValue(vm => vm.IsEnabled, e => e && Plugin.ConfigurationDialog != null));

        this.WhenActivated(d =>
        {
            pluginManagementService.PluginFeatureEnabled += PluginManagementServiceOnPluginFeatureChanged;
            pluginManagementService.PluginFeatureDisabled += PluginManagementServiceOnPluginFeatureChanged;

            Disposable.Create(() =>
            {
                pluginManagementService.PluginFeatureEnabled -= PluginManagementServiceOnPluginFeatureChanged;
                pluginManagementService.PluginFeatureDisabled -= PluginManagementServiceOnPluginFeatureChanged;
                _settingsWindow?.Close();
            }).DisposeWith(d);
        });
    }

    public ReactiveCommand<Unit, Unit> OpenSettings { get; }

    public ObservableCollection<PluginPlatformViewModel> Platforms { get; }

    public Plugin Plugin { get; }
    public PluginFeatureInfo PluginFeature { get; }
    public bool IsEnabled => PluginFeature.Instance != null && PluginFeature.Instance.IsEnabled;

    public async Task UpdateEnabled(bool enable)
    {
        if (Enabling)
            return;

        if (!enable)
        {
            try
            {
                if (PluginFeature.AlwaysEnabled)
                    await Task.Run(() => _pluginManagementService.DisablePlugin(Plugin, true));
                else if (PluginFeature.Instance != null)
                    await Task.Run(() => _pluginManagementService.DisablePluginFeature(PluginFeature.Instance, true));
            }
            catch (Exception e)
            {
                await ShowUpdateEnableFailure(enable, e);
            }
            finally
            {
                this.RaisePropertyChanged(nameof(IsEnabled));
            }

            return;
        }

        try
        {
            Enabling = true;
            if (Plugin.Info.RequiresAdmin && !_coreService.IsElevated)
            {
                bool confirmed = await _windowService.ShowConfirmContentDialog("Enable feature", "This feature requires admin rights, are you sure you want to enable it?");
                if (!confirmed)
                    return;
            }

            // Check if all prerequisites are met async
            List<IPrerequisitesSubject> subjects = new() {Plugin.Info};
            subjects.AddRange(Plugin.Features.Where(f => f.AlwaysEnabled || f.EnabledInStorage));

            if (subjects.Any(s => !s.ArePrerequisitesMet()))
            {
                await PluginPrerequisitesInstallDialogViewModel.Show(_windowService, subjects);
                if (!subjects.All(s => s.ArePrerequisitesMet()))
                    return;
            }

            await Task.Run(() =>
            {
                if (!Plugin.IsEnabled)
                    _pluginManagementService.EnablePlugin(Plugin, true, true);
                if (PluginFeature.Instance != null && !PluginFeature.Instance.IsEnabled)
                    _pluginManagementService.EnablePluginFeature(PluginFeature.Instance, true);
            });
        }
        catch (Exception e)
        {
            await ShowUpdateEnableFailure(enable, e);
        }
        finally
        {
            Enabling = false;
            this.RaisePropertyChanged(nameof(IsEnabled));
        }
    }

    private void ExecuteOpenSettings()
    {
        if (Plugin.ConfigurationDialog == null)
            return;

        if (_settingsWindow != null)
        {
            _settingsWindow.WindowState = WindowState.Normal;
            _settingsWindow.Activate();
            return;
        }

        try
        {
            if (Plugin.Resolve(Plugin.ConfigurationDialog.Type) is not PluginConfigurationViewModel viewModel)
                throw new ArtemisUIException($"The type of a plugin configuration dialog must inherit {nameof(PluginConfigurationViewModel)}");

            _settingsWindow = _windowService.ShowWindow(new PluginSettingsWindowViewModel(viewModel));
            _settingsWindow.Closed += (_, _) => _settingsWindow = null;
        }
        catch (Exception e)
        {
            _windowService.ShowExceptionDialog("An exception occured while trying to show the plugin's settings window", e);
            throw;
        }
    }

    private async Task ShowUpdateEnableFailure(bool enable, Exception e)
    {
        string action = enable ? "enable" : "disable";
        ContentDialogBuilder builder = _windowService.CreateContentDialog()
            .WithTitle($"Failed to {action} plugin {Plugin.Info.Name}")
            .WithContent(e.Message)
            .HavingPrimaryButton(b => b.WithText("View logs").WithAction(ShowLogsFolder));
        // If available, add a secondary button pointing to the support page
        if (Plugin.Info.HelpPage != null)
            builder = builder.HavingSecondaryButton(b => b.WithText("Open support page").WithAction(() => Utilities.OpenUrl(Plugin.Info.HelpPage.ToString())));

        await builder.ShowAsync();
    }

    private void ShowLogsFolder()
    {
        try
        {
            Utilities.OpenFolder(Constants.LogsFolder);
        }
        catch (Exception e)
        {
            _windowService.ShowExceptionDialog("Welp, we couldn\'t open the logs folder for you", e);
        }
    }

    private void PluginManagementServiceOnPluginFeatureChanged(object? sender, PluginFeatureEventArgs e)
    {
        if (e.PluginFeature.Info != PluginFeature)
            return;
        
        Dispatcher.UIThread.Post(() =>
        {
            this.RaisePropertyChanged(nameof(IsEnabled));
            if (!IsEnabled)
                _settingsWindow?.Close();
        });
    }
}