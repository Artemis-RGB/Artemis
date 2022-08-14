using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Screens.Scripting;
using Artemis.UI.Screens.Sidebar;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.ProfileEditor;
using Newtonsoft.Json;
using ReactiveUI;
using Serilog;

namespace Artemis.UI.Screens.ProfileEditor.MenuBar;

public class MenuBarViewModel : ActivatableViewModelBase
{
    private readonly ILogger _logger;
    private readonly IProfileEditorService _profileEditorService;
    private readonly IProfileService _profileService;
    private readonly ISettingsService _settingsService;
    private readonly IWindowService _windowService;
    private ProfileEditorHistory? _history;
    private ObservableAsPropertyHelper<bool>? _isSuspended;
    private ObservableAsPropertyHelper<ProfileConfiguration?>? _profileConfiguration;
    private ObservableAsPropertyHelper<RenderProfileElement?>? _profileElement;
    private ObservableAsPropertyHelper<bool>? _suspendedEditing;

    public MenuBarViewModel(ILogger logger, IProfileEditorService profileEditorService, IProfileService profileService, ISettingsService settingsService, IWindowService windowService)
    {
        _logger = logger;
        _profileEditorService = profileEditorService;
        _profileService = profileService;
        _settingsService = settingsService;
        _windowService = windowService;
        this.WhenActivated(d =>
        {
            profileEditorService.History.Subscribe(history => History = history).DisposeWith(d);
            _profileConfiguration = profileEditorService.ProfileConfiguration.ToProperty(this, vm => vm.ProfileConfiguration).DisposeWith(d);
            _profileElement = profileEditorService.ProfileElement.ToProperty(this, vm => vm.ProfileElement).DisposeWith(d);
            _suspendedEditing = profileEditorService.SuspendedEditing.ToProperty(this, vm => vm.SuspendedEditing).DisposeWith(d);
            _isSuspended = profileEditorService.ProfileConfiguration
                .Select(p => p?.WhenAnyValue(c => c.IsSuspended) ?? Observable.Never<bool>())
                .Switch()
                .ToProperty(this, vm => vm.IsSuspended)
                .DisposeWith(d);
        });

        AddFolder = ReactiveCommand.Create(ExecuteAddFolder);
        AddLayer = ReactiveCommand.Create(ExecuteAddLayer);
        ViewProperties = ReactiveCommand.CreateFromTask(ExecuteViewProperties, this.WhenAnyValue(vm => vm.ProfileConfiguration).Select(c => c != null));
        ViewScripts = ReactiveCommand.CreateFromTask(ExecuteViewScripts, this.WhenAnyValue(vm => vm.ProfileConfiguration).Select(c => c != null));
        AdaptProfile = ReactiveCommand.CreateFromTask(ExecuteAdaptProfile, this.WhenAnyValue(vm => vm.ProfileConfiguration).Select(c => c != null));
        ToggleSuspended = ReactiveCommand.Create(ExecuteToggleSuspended, this.WhenAnyValue(vm => vm.ProfileConfiguration).Select(c => c != null));
        DeleteProfile = ReactiveCommand.CreateFromTask(ExecuteDeleteProfile, this.WhenAnyValue(vm => vm.ProfileConfiguration).Select(c => c != null));
        ExportProfile = ReactiveCommand.CreateFromTask(ExecuteExportProfile, this.WhenAnyValue(vm => vm.ProfileConfiguration).Select(c => c != null));
        DuplicateProfile = ReactiveCommand.Create(ExecuteDuplicateProfile, this.WhenAnyValue(vm => vm.ProfileConfiguration).Select(c => c != null));
        ToggleSuspendedEditing = ReactiveCommand.Create(ExecuteToggleSuspendedEditing);
        ToggleBooleanSetting = ReactiveCommand.Create<PluginSetting<bool>>(ExecuteToggleBooleanSetting);
        OpenUri = ReactiveCommand.Create<string>(s => Process.Start(new ProcessStartInfo(s) {UseShellExecute = true, Verb = "open"}));
    }

    public ReactiveCommand<Unit, Unit> AddFolder { get; }
    public ReactiveCommand<Unit, Unit> AddLayer { get; }
    public ReactiveCommand<Unit, Unit> ToggleSuspended { get; }
    public ReactiveCommand<Unit, Unit> ViewProperties { get; }
    public ReactiveCommand<Unit, Unit> ViewScripts { get; }
    public ReactiveCommand<Unit, Unit> AdaptProfile { get; }
    public ReactiveCommand<Unit, Unit> DeleteProfile { get; }
    public ReactiveCommand<Unit, Unit> ExportProfile { get; }
    public ReactiveCommand<Unit, Unit> DuplicateProfile { get; }
    public ReactiveCommand<PluginSetting<bool>, Unit> ToggleBooleanSetting { get; }
    public ReactiveCommand<string, Unit> OpenUri { get; }
    public ReactiveCommand<Unit, Unit> ToggleSuspendedEditing { get; }

    public ProfileConfiguration? ProfileConfiguration => _profileConfiguration?.Value;
    public RenderProfileElement? ProfileElement => _profileElement?.Value;
    public bool IsSuspended => _isSuspended?.Value ?? false;
    public bool SuspendedEditing => _suspendedEditing?.Value ?? false;

    public PluginSetting<bool> AutoSuspend => _settingsService.GetSetting("ProfileEditor.AutoSuspend", true);
    public PluginSetting<bool> FocusSelectedLayer => _settingsService.GetSetting("ProfileEditor.FocusSelectedLayer", false);
    public PluginSetting<bool> ShowDataModelValues => _settingsService.GetSetting("ProfileEditor.ShowDataModelValues", false);
    public PluginSetting<bool> ShowFullPaths => _settingsService.GetSetting("ProfileEditor.ShowFullPaths", false);
    public PluginSetting<bool> AlwaysShowValues => _settingsService.GetSetting("ProfileEditor.AlwaysShowValues", true);
    public PluginSetting<bool> AlwaysApplyDataBindings => _settingsService.GetSetting("ProfileEditor.AlwaysApplyDataBindings", false);

    public ProfileEditorHistory? History
    {
        get => _history;
        set => RaiseAndSetIfChanged(ref _history, value);
    }

    private void ExecuteAddFolder()
    {
        if (ProfileConfiguration?.Profile == null)
            return;

        RenderProfileElement target = ProfileElement ?? ProfileConfiguration.Profile.GetRootFolder();
        _profileEditorService.CreateAndAddFolder(target);
    }

    private void ExecuteAddLayer()
    {
        if (ProfileConfiguration?.Profile == null)
            return;

        RenderProfileElement target = ProfileElement ?? ProfileConfiguration.Profile.GetRootFolder();
        _profileEditorService.CreateAndAddLayer(target);
    }

    private async Task ExecuteViewProperties()
    {
        if (ProfileConfiguration == null)
            return;

        await _windowService.ShowDialogAsync<ProfileConfigurationEditViewModel, ProfileConfiguration?>(
            ("profileCategory", ProfileConfiguration.Category),
            ("profileConfiguration", ProfileConfiguration)
        );
    }
    
    private async Task ExecuteViewScripts()
    {
        if (ProfileConfiguration?.Profile == null)
            return;

        await _windowService.ShowDialogAsync<ScriptsDialogViewModel, object?>(("profile", ProfileConfiguration.Profile));
        await _profileEditorService.SaveProfileAsync();
    }

    private async Task ExecuteAdaptProfile()
    {
        if (ProfileConfiguration?.Profile == null)
            return;

        bool confirmed = await _windowService.ShowConfirmContentDialog(
            "Adapt profile",
            "Are you sure you want to adapt the profile to your current surface? Layer assignments may change and you will lose your undo/redo history."
        );
        if (!confirmed)
            return;

        _profileService.AdaptProfile(ProfileConfiguration.Profile);
        _history?.Clear();
    }

    private void ExecuteToggleSuspended()
    {
        if (ProfileConfiguration == null)
            return;

        ProfileConfiguration.IsSuspended = !ProfileConfiguration.IsSuspended;
        _profileService.SaveProfileCategory(ProfileConfiguration.Category);
    }

    private async Task ExecuteDeleteProfile()
    {
        if (ProfileConfiguration == null)
            return;
        if (!await _windowService.ShowConfirmContentDialog("Delete profile", "Are you sure you want to permanently delete this profile?"))
            return;

        if (ProfileConfiguration.IsBeingEdited)
            _profileEditorService.ChangeCurrentProfileConfiguration(null);
        _profileService.RemoveProfileConfiguration(ProfileConfiguration);
    }

    private async Task ExecuteExportProfile()
    {
        if (ProfileConfiguration == null)
            return;

        // Might not cover everything but then the dialog will complain and that's good enough
        string fileName = Path.GetInvalidFileNameChars().Aggregate(ProfileConfiguration.Name, (current, c) => current.Replace(c, '-'));
        string? result = await _windowService.CreateSaveFileDialog()
            .HavingFilter(f => f.WithExtension("json").WithName("Artemis profile"))
            .WithInitialFileName(fileName)
            .ShowAsync();

        if (result == null)
            return;

        ProfileConfigurationExportModel export = _profileService.ExportProfile(ProfileConfiguration);
        string json = JsonConvert.SerializeObject(export, IProfileService.ExportSettings);
        try
        {
            await File.WriteAllTextAsync(result, json);
        }
        catch (Exception e)
        {
            _windowService.ShowExceptionDialog("Failed to export profile", e);
        }
    }

    private void ExecuteDuplicateProfile()
    {
        if (ProfileConfiguration == null)
            return;

        ProfileConfigurationExportModel export = _profileService.ExportProfile(ProfileConfiguration);
        _profileService.ImportProfile(ProfileConfiguration.Category, export, true, false, "copy");
    }

    private void ExecuteToggleSuspendedEditing()
    {
        _profileEditorService.ChangeSuspendedEditing(!SuspendedEditing);
    }

    private void ExecuteToggleBooleanSetting(PluginSetting<bool> setting)
    {
        setting.Value = !setting.Value;
        setting.Save();
    }
}