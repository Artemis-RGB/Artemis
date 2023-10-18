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
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.ProfileEditor;
using Newtonsoft.Json;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.MenuBar;

public partial class MenuBarViewModel : ActivatableViewModelBase
{
    private readonly IRouter _router;
    private readonly IProfileEditorService _profileEditorService;
    private readonly IProfileService _profileService;
    private readonly ISettingsService _settingsService;
    private readonly IWindowService _windowService;
    private ObservableAsPropertyHelper<bool>? _focusFolder;
    private ObservableAsPropertyHelper<bool>? _focusNone;
    private ObservableAsPropertyHelper<bool>? _focusSelection;
    private ObservableAsPropertyHelper<bool>? _isSuspended;
    private ObservableAsPropertyHelper<bool>? _keyBindingsEnabled;
    private ObservableAsPropertyHelper<ProfileConfiguration?>? _profileConfiguration;
    private ObservableAsPropertyHelper<RenderProfileElement?>? _profileElement;
    private ObservableAsPropertyHelper<bool>? _suspendedEditing;
    [Notify] private ProfileEditorHistory? _history;

    public MenuBarViewModel(IRouter router, IProfileEditorService profileEditorService, IProfileService profileService, ISettingsService settingsService, IWindowService windowService)
    {
        _router = router;
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

            _focusNone = profileEditorService.FocusMode.Select(f => f == ProfileEditorFocusMode.None).ToProperty(this, vm => vm.FocusNone).DisposeWith(d);
            _focusFolder = profileEditorService.FocusMode.Select(f => f == ProfileEditorFocusMode.Folder).ToProperty(this, vm => vm.FocusFolder).DisposeWith(d);
            _focusSelection = profileEditorService.FocusMode.Select(f => f == ProfileEditorFocusMode.Selection).ToProperty(this, vm => vm.FocusSelection).DisposeWith(d);
            _keyBindingsEnabled = Shared.UI.CurrentKeyBindingsEnabled.ToProperty(this, vm => vm.KeyBindingsEnabled).DisposeWith(d);
        });

        AddFolder = ReactiveCommand.Create(ExecuteAddFolder);
        AddLayer = ReactiveCommand.Create(ExecuteAddLayer);
        ViewProperties = ReactiveCommand.CreateFromTask(ExecuteViewProperties, this.WhenAnyValue(vm => vm.ProfileConfiguration).Select(c => c != null));
        ViewScripts = ReactiveCommand.CreateFromTask(ExecuteViewScripts, this.WhenAnyValue(vm => vm.ProfileConfiguration).Select(c => c != null));
        AdaptProfile = ReactiveCommand.CreateFromTask(ExecuteAdaptProfile, this.WhenAnyValue(vm => vm.ProfileConfiguration).Select(c => c != null));
        ToggleSuspended = ReactiveCommand.Create(ExecuteToggleSuspended, this.WhenAnyValue(vm => vm.ProfileConfiguration).Select(c => c != null));
        DeleteProfile = ReactiveCommand.CreateFromTask(ExecuteDeleteProfile, this.WhenAnyValue(vm => vm.ProfileConfiguration).Select(c => c != null));
        ExportProfile = ReactiveCommand.CreateFromTask(ExecuteExportProfile, this.WhenAnyValue(vm => vm.ProfileConfiguration).Select(c => c != null));
        DuplicateProfile = ReactiveCommand.CreateFromTask(ExecuteDuplicateProfile, this.WhenAnyValue(vm => vm.ProfileConfiguration).Select(c => c != null));
        ToggleSuspendedEditing = ReactiveCommand.Create(ExecuteToggleSuspendedEditing);
        OpenUri = ReactiveCommand.Create<string>(s => Process.Start(new ProcessStartInfo(s) {UseShellExecute = true, Verb = "open"}));
        ToggleBooleanSetting = ReactiveCommand.Create<PluginSetting<bool>>(ExecuteToggleBooleanSetting);
        ChangeFocusMode = ReactiveCommand.Create<ProfileEditorFocusMode>(ExecuteChangeFocusMode);
        CycleFocusMode = ReactiveCommand.Create(ExecuteCycleFocusMode, this.WhenAnyValue(vm => vm.KeyBindingsEnabled));
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
    public ReactiveCommand<ProfileEditorFocusMode, Unit> ChangeFocusMode { get; }
    public ReactiveCommand<Unit, Unit> CycleFocusMode { get; }

    public ProfileConfiguration? ProfileConfiguration => _profileConfiguration?.Value;
    public RenderProfileElement? ProfileElement => _profileElement?.Value;
    public bool IsSuspended => _isSuspended?.Value ?? false;
    public bool SuspendedEditing => _suspendedEditing?.Value ?? false;
    public bool FocusNone => _focusNone?.Value ?? false;
    public bool FocusFolder => _focusFolder?.Value ?? false;
    public bool FocusSelection => _focusSelection?.Value ?? false;
    public bool KeyBindingsEnabled => _keyBindingsEnabled?.Value ?? false;

    public PluginSetting<bool> AutoSuspend => _settingsService.GetSetting("ProfileEditor.AutoSuspend", true);
    public PluginSetting<bool> ShowDataModelValues => _settingsService.GetSetting("ProfileEditor.ShowDataModelValues", false);
    public PluginSetting<bool> ShowFullPaths => _settingsService.GetSetting("ProfileEditor.ShowFullPaths", false);
    public PluginSetting<bool> AlwaysShowValues => _settingsService.GetSetting("ProfileEditor.AlwaysShowValues", true);
    public PluginSetting<bool> AlwaysApplyDataBindings => _settingsService.GetSetting("ProfileEditor.AlwaysApplyDataBindings", false);
    public PluginSetting<ProfileEditorFocusMode> FocusMode => _settingsService.GetSetting("ProfileEditor.FocusMode", ProfileEditorFocusMode.Folder);

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

        await _windowService.ShowDialogAsync<ProfileConfigurationEditViewModel, ProfileConfiguration?>(ProfileConfiguration.Category, ProfileConfiguration);
    }

    private async Task ExecuteViewScripts()
    {
        if (ProfileConfiguration?.Profile == null)
            return;

        await _windowService.ShowDialogAsync<ScriptsDialogViewModel, object?>(ProfileConfiguration.Profile);
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

        if (_profileService.FocusProfile == ProfileConfiguration)
            await _router.Navigate("home");
        _profileService.RemoveProfileConfiguration(ProfileConfiguration);
    }

    private async Task ExecuteExportProfile()
    {
        if (ProfileConfiguration == null)
            return;

        // Might not cover everything but then the dialog will complain and that's good enough
        string fileName = Path.GetInvalidFileNameChars().Aggregate(ProfileConfiguration.Name, (current, c) => current.Replace(c, '-'));
        string? result = await _windowService.CreateSaveFileDialog()
            .HavingFilter(f => f.WithExtension("zip").WithName("Artemis profile"))
            .WithInitialFileName(fileName)
            .ShowAsync();

        if (result == null)
            return;

        try
        {
            await using Stream stream = await _profileService.ExportProfile(ProfileConfiguration);
            await using FileStream fileStream = File.OpenWrite(result);
            await stream.CopyToAsync(fileStream);
        }
        catch (Exception e)
        {
            _windowService.ShowExceptionDialog("Failed to export profile", e);
        }
    }
    
    private async Task ExecuteDuplicateProfile()
    {
        if (ProfileConfiguration == null)
            return;
        await using Stream export = await _profileService.ExportProfile(ProfileConfiguration);
        await _profileService.ImportProfile(export, ProfileConfiguration.Category, true, false, "copy");
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

    private void ExecuteCycleFocusMode()
    {
        if (FocusMode.Value == ProfileEditorFocusMode.None)
            FocusMode.Value = ProfileEditorFocusMode.Folder;
        else if (FocusMode.Value == ProfileEditorFocusMode.Folder)
            FocusMode.Value = ProfileEditorFocusMode.Selection;
        else if (FocusMode.Value == ProfileEditorFocusMode.Selection)
            FocusMode.Value = ProfileEditorFocusMode.None;

        FocusMode.Save();
        _profileEditorService.ChangeFocusMode(FocusMode.Value);
    }

    private void ExecuteChangeFocusMode(ProfileEditorFocusMode focusMode)
    {
        FocusMode.Value = focusMode;
        FocusMode.Save();
        _profileEditorService.ChangeFocusMode(FocusMode.Value);
    }
}