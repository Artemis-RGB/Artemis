using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Exceptions;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Builders;
using Avalonia.Controls;
using Avalonia.Threading;
using DryIoc;
using Material.Icons;
using ReactiveUI;

namespace Artemis.UI.Screens.Plugins;

public class PluginViewModel : ActivatableViewModelBase
{
    private readonly ICoreService _coreService;
    private readonly INotificationService _notificationService;
    private readonly IPluginManagementService _pluginManagementService;
    private readonly IWindowService _windowService;
    private bool _canInstallPrerequisites;
    private bool _canRemovePrerequisites;
    private bool _enabling;
    private Plugin _plugin;
    private Window? _window;

    public PluginViewModel(Plugin plugin,
        ReactiveCommand<Unit, Unit>? reload,
        ICoreService coreService,
        IWindowService windowService,
        INotificationService notificationService,
        IPluginManagementService pluginManagementService)
    {
        _plugin = plugin;
        _coreService = coreService;
        _windowService = windowService;
        _notificationService = notificationService;
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
                _window?.Close();
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

    public Plugin Plugin
    {
        get => _plugin;
        set => RaiseAndSetIfChanged(ref _plugin, value);
    }

    public bool Enabling
    {
        get => _enabling;
        set => RaiseAndSetIfChanged(ref _enabling, value);
    }

    public string Type => Plugin.GetType().BaseType?.Name ?? Plugin.GetType().Name;
    public bool IsEnabled => Plugin.IsEnabled;

    public bool CanInstallPrerequisites
    {
        get => _canInstallPrerequisites;
        set => RaiseAndSetIfChanged(ref _canInstallPrerequisites, value);
    }

    public bool CanRemovePrerequisites
    {
        get => _canRemovePrerequisites;
        set => RaiseAndSetIfChanged(ref _canRemovePrerequisites, value);
    }

    public async Task UpdateEnabled(bool enable)
    {
        if (Enabling)
            return;

        if (!enable)
        {
            try
            {
                await Task.Run(() => _pluginManagementService.DisablePlugin(Plugin, true));
            }
            catch (Exception e)
            {
                await Dispatcher.UIThread.InvokeAsync(() => _notificationService.CreateNotification()
                    .WithSeverity(NotificationSeverity.Error)
                    .WithMessage($"Failed to disable plugin {Plugin.Info.Name}\r\n{e.Message}")
                    .HavingButton(b => b.WithText("View logs").WithCommand(ShowLogsFolder))
                    .Show());
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
                bool confirmed = await _windowService.ShowConfirmContentDialog("Enable plugin", "This plugin requires admin rights, are you sure you want to enable it?");
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

            await Task.Run(() => _pluginManagementService.EnablePlugin(Plugin, true, true));
        }
        catch (Exception e)
        {
            await Dispatcher.UIThread.InvokeAsync(() => _notificationService.CreateNotification()
                .WithSeverity(NotificationSeverity.Error)
                .WithMessage($"Failed to enable plugin {Plugin.Info.Name}\r\n{e.Message}")
                .HavingButton(b => b.WithText("View logs").WithCommand(ShowLogsFolder))
                .Show());
        }
        finally
        {
            Enabling = false;
            this.RaisePropertyChanged(nameof(IsEnabled));
        }
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

        if (_window != null)
        {
            _window.WindowState = WindowState.Normal;
            _window.Activate();
            return;
        }

        try
        {
            if (Plugin.Resolve(Plugin.ConfigurationDialog.Type) is not PluginConfigurationViewModel viewModel)
                throw new ArtemisUIException($"The type of a plugin configuration dialog must inherit {nameof(PluginConfigurationViewModel)}");

            _window = _windowService.ShowWindow(new PluginSettingsWindowViewModel(viewModel));
            _window.Closed += (_, _) => _window = null;
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

    private async Task ExecuteRemovePrerequisites(bool forPluginRemoval = false)
    {
        List<IPrerequisitesSubject> subjects = new() {Plugin.Info};
        subjects.AddRange(!forPluginRemoval ? Plugin.Features.Where(f => f.AlwaysEnabled) : Plugin.Features);

        if (subjects.Any(s => s.PlatformPrerequisites.Any(p => p.UninstallActions.Any())))
            await PluginPrerequisitesUninstallDialogViewModel.Show(_windowService, subjects, forPluginRemoval ? "Skip, remove plugin" : "Cancel");
    }

    private async Task ExecuteRemoveSettings()
    {
        bool confirmed = await _windowService.ShowConfirmContentDialog("Clear plugin settings", "Are you sure you want to clear the settings of this plugin?");
        if (!confirmed)
            return;

        bool wasEnabled = IsEnabled;

        if (IsEnabled)
            await UpdateEnabled(false);

        _pluginManagementService.RemovePluginSettings(Plugin);

        if (wasEnabled)
            await UpdateEnabled(true);

        _notificationService.CreateNotification().WithTitle("Cleared plugin settings.").Show();
    }

    private async Task ExecuteRemove()
    {
        bool confirmed = await _windowService.ShowConfirmContentDialog("Remove plugin", "Are you sure you want to remove this plugin?");
        if (!confirmed)
            return;

        // If the plugin or any of its features has uninstall actions, offer to run these
        List<IPrerequisitesSubject> subjects = new() {Plugin.Info};
        subjects.AddRange(Plugin.Features);
        if (subjects.Any(s => s.PlatformPrerequisites.Any(p => p.UninstallActions.Any())))
            await ExecuteRemovePrerequisites(true);

        try
        {
            _pluginManagementService.RemovePlugin(Plugin, false);
        }
        catch (Exception e)
        {
            _windowService.ShowExceptionDialog("Failed to remove plugin", e);
            throw;
        }

        _notificationService.CreateNotification().WithTitle("Removed plugin.").Show();
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
                _window?.Close();
        });
    }
}