using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Exceptions;
using Artemis.UI.Services;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Avalonia.Controls;
using Avalonia.Threading;
using Material.Icons;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.Plugins;

public partial class PluginViewModel : ActivatableViewModelBase
{
    private readonly IPluginInteractionService _pluginInteractionService;
    private readonly IWindowService _windowService;
    private Window? _settingsWindow;
    [Notify] private bool _canInstallPrerequisites;
    [Notify] private bool _canRemovePrerequisites;
    [Notify] private bool _enabling;
    [Notify] private Plugin _plugin;

    public PluginViewModel(Plugin plugin, ReactiveCommand<Unit, Unit>? reload, IWindowService windowService, IPluginInteractionService pluginInteractionService)
    {
        _plugin = plugin;
        _windowService = windowService;
        _pluginInteractionService = pluginInteractionService;

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

        Reload = reload;
        OpenSettings = ReactiveCommand.Create(ExecuteOpenSettings, this.WhenAnyValue(vm => vm.IsEnabled, e => e && Plugin.ConfigurationDialog != null));
        RemoveSettings = ReactiveCommand.CreateFromTask(ExecuteRemoveSettings);
        Remove = ReactiveCommand.CreateFromTask(ExecuteRemove);
        InstallPrerequisites = ReactiveCommand.CreateFromTask(ExecuteInstallPrerequisites, this.WhenAnyValue(x => x.CanInstallPrerequisites));
        RemovePrerequisites = ReactiveCommand.CreateFromTask<bool>(ExecuteRemovePrerequisites, this.WhenAnyValue(x => x.CanRemovePrerequisites));
        ShowLogsFolder = ReactiveCommand.Create(ExecuteShowLogsFolder);
        OpenPluginDirectory = ReactiveCommand.Create(ExecuteOpenPluginDirectory);

        this.WhenActivated(d =>
        {
            Plugin.Enabled += OnPluginToggled;
            Plugin.Disabled += OnPluginToggled;

            Disposable.Create(() =>
            {
                Plugin.Enabled -= OnPluginToggled;
                Plugin.Disabled -= OnPluginToggled;
                _settingsWindow?.Close();
            }).DisposeWith(d);
        });
    }

    public ReactiveCommand<Unit, Unit>? Reload { get; }
    public ReactiveCommand<Unit, Unit> OpenSettings { get; }
    public ReactiveCommand<Unit, Unit> RemoveSettings { get; }
    public ReactiveCommand<Unit, Unit> Remove { get; }
    public ReactiveCommand<Unit, Unit> InstallPrerequisites { get; }
    public ReactiveCommand<bool, Unit> RemovePrerequisites { get; }
    public ReactiveCommand<Unit, Unit> ShowLogsFolder { get; }
    public ReactiveCommand<Unit, Unit> OpenPluginDirectory { get; }

    public ObservableCollection<PluginPlatformViewModel> Platforms { get; }
    public bool IsEnabled => Plugin.IsEnabled;

    public async Task UpdateEnabled(bool enable)
    {
        if (Enabling)
            return;

        if (!enable)
            await _pluginInteractionService.DisablePlugin(Plugin);
        else
        {
            Enabling = true;
            await _pluginInteractionService.EnablePlugin(Plugin, false);
            Enabling = false;
        }

        this.RaisePropertyChanged(nameof(IsEnabled));
    }

    public void CheckPrerequisites()
    {
        CanInstallPrerequisites = false;
        CanRemovePrerequisites = false;

        Dispatcher.UIThread.Post(() =>
        {
            CanInstallPrerequisites = !Plugin.Info.ArePrerequisitesMet() || Plugin.Features.Where(f => f.AlwaysEnabled).Any(f => !f.ArePrerequisitesMet());
            CanRemovePrerequisites = Plugin.Info.PlatformPrerequisites.Any(p => p.IsMet() && p.UninstallActions.Any()) ||
                                     Plugin.Features.Where(f => f.AlwaysEnabled).Any(f => f.PlatformPrerequisites.Any(p => p.IsMet() && p.UninstallActions.Any()));
        });
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

    private void ExecuteOpenPluginDirectory()
    {
        try
        {
            Utilities.OpenFolder(Plugin.Directory.FullName);
        }
        catch (Exception e)
        {
            _windowService.ShowExceptionDialog("Welp, we couldn't open the device's plugin folder for you", e);
        }
    }

    private async Task ExecuteInstallPrerequisites()
    {
        List<IPrerequisitesSubject> subjects = new() {Plugin.Info};
        subjects.AddRange(Plugin.Features.Where(f => f.AlwaysEnabled));

        if (subjects.Any(s => s.PlatformPrerequisites.Any()))
            await PluginPrerequisitesInstallDialogViewModel.Show(_windowService, subjects);
    }

    public async Task ExecuteRemovePrerequisites(bool forPluginRemoval = false)
    {
        List<IPrerequisitesSubject> subjects = new() {Plugin.Info};
        subjects.AddRange(!forPluginRemoval ? Plugin.Features.Where(f => f.AlwaysEnabled) : Plugin.Features);

        if (subjects.Any(s => s.PlatformPrerequisites.Any(p => p.UninstallActions.Any())))
            await PluginPrerequisitesUninstallDialogViewModel.Show(_windowService, subjects, forPluginRemoval ? "Skip, remove plugin" : "Cancel");
    }

    private async Task ExecuteRemoveSettings()
    {
        await _pluginInteractionService.RemovePluginSettings(Plugin);
    }

    private async Task ExecuteRemove()
    {
        await _pluginInteractionService.RemovePlugin(Plugin);
    }

    private void ExecuteShowLogsFolder()
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

    private void OnPluginToggled(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            this.RaisePropertyChanged(nameof(IsEnabled));
            if (!IsEnabled)
                _settingsWindow?.Close();
        });
    }

    public async Task AutoEnable()
    {
        if (IsEnabled)
            return;

        await UpdateEnabled(true);

        // If enabling failed, don't offer to show the settings
        if (!IsEnabled || Plugin.ConfigurationDialog == null)
            return;

        if (await _windowService.ShowConfirmContentDialog("Open plugin settings", "This plugin has settings, would you like to view them?", "Yes", "No"))
            ExecuteOpenSettings();
    }
}