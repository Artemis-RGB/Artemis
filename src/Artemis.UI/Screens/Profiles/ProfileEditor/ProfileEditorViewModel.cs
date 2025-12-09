using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Screens.Profiles.ProfileEditor.DisplayCondition;
using Artemis.UI.Screens.Profiles.ProfileEditor.ProfileTree;
using Artemis.UI.Screens.Profiles.ProfileEditor.Properties;
using Artemis.UI.Screens.Profiles.ProfileEditor.StatusBar;
using Artemis.UI.Screens.Profiles.ProfileEditor.VisualEditor;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.MainWindow;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.WebClient.Workshop.Models;
using Artemis.WebClient.Workshop.Services;
using DynamicData;
using DynamicData.Binding;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.Profiles.ProfileEditor;

public partial class ProfileEditorViewModel : RoutableScreen<ProfileEditorViewModelParameters>, IMainScreenViewModel
{
    private readonly IProfileEditorService _profileEditorService;
    private readonly IProfileService _profileService;
    private readonly ISettingsService _settingsService;
    private readonly IMainWindowService _mainWindowService;
    private readonly IWorkshopService _workshopService;
    private readonly IWindowService _windowService;
    private readonly SourceList<IToolViewModel> _tools;
    private ObservableAsPropertyHelper<ProfileEditorHistory?>? _history;
    private ObservableAsPropertyHelper<bool>? _suspendedEditing;

    [Notify] private ProfileConfiguration? _profileConfiguration;

    /// <inheritdoc />
    public ProfileEditorViewModel(IProfileService profileService,
        IProfileEditorService profileEditorService,
        ISettingsService settingsService,
        VisualEditorViewModel visualEditorViewModel,
        ProfileTreeViewModel profileTreeViewModel,
        ProfileEditorTitleBarViewModel profileEditorTitleBarViewModel,
        PropertiesViewModel propertiesViewModel,
        DisplayConditionScriptViewModel displayConditionScriptViewModel,
        StatusBarViewModel statusBarViewModel,
        IEnumerable<IToolViewModel> toolViewModels,
        IMainWindowService mainWindowService,
        IInputService inputService,
        IWorkshopService workshopService,
        IWindowService windowService)
    {
        _profileService = profileService;
        _profileEditorService = profileEditorService;
        _settingsService = settingsService;
        _mainWindowService = mainWindowService;
        _workshopService = workshopService;
        _windowService = windowService;

        _tools = new SourceList<IToolViewModel>();
        _tools.AddRange(toolViewModels);
        _tools.Connect().AutoRefreshOnObservable(t => t.WhenAnyValue(vm => vm.IsSelected)).Subscribe(OnToolSelected);
        _tools.Connect()
            .Filter(t => t.ShowInToolbar)
            .Sort(SortExpressionComparer<IToolViewModel>.Ascending(vm => vm.Order))
            .Bind(out ReadOnlyObservableCollection<IToolViewModel> tools)
            .Subscribe();
        Tools = tools;
        visualEditorViewModel.SetTools(_tools);

        StatusBarViewModel = statusBarViewModel;
        VisualEditorViewModel = visualEditorViewModel;
        ProfileTreeViewModel = profileTreeViewModel;
        PropertiesViewModel = propertiesViewModel;
        DisplayConditionScriptViewModel = displayConditionScriptViewModel;

        this.WhenActivated(d =>
        {
            _history = profileEditorService.History.ToProperty(this, vm => vm.History).DisposeWith(d);
            _suspendedEditing = profileEditorService.SuspendedEditing.ToProperty(this, vm => vm.SuspendedEditing).DisposeWith(d);

            inputService.KeyboardKeyDown += InputServiceOnKeyboardKeyDown;
            mainWindowService.MainWindowFocused += MainWindowServiceOnMainWindowFocused;
            mainWindowService.MainWindowUnfocused += MainWindowServiceOnMainWindowUnfocused;

            Disposable.Create(() =>
            {
                inputService.KeyboardKeyDown -= InputServiceOnKeyboardKeyDown;
                mainWindowService.MainWindowFocused -= MainWindowServiceOnMainWindowFocused;
                mainWindowService.MainWindowUnfocused -= MainWindowServiceOnMainWindowUnfocused;
                foreach (IToolViewModel toolViewModel in _tools.Items)
                    toolViewModel.Dispose();
            }).DisposeWith(d);
        });

        TitleBarViewModel = profileEditorTitleBarViewModel;
        ToggleSuspend = ReactiveCommand.Create(ExecuteToggleSuspend);
        ToggleAutoSuspend = ReactiveCommand.Create(ExecuteToggleAutoSuspend);
    }

    public VisualEditorViewModel? VisualEditorViewModel { get; }
    public ProfileTreeViewModel? ProfileTreeViewModel { get; }
    public PropertiesViewModel? PropertiesViewModel { get; }
    public DisplayConditionScriptViewModel? DisplayConditionScriptViewModel { get; }
    public StatusBarViewModel? StatusBarViewModel { get; }

    public ReadOnlyObservableCollection<IToolViewModel> Tools { get; }
    public ProfileEditorHistory? History => _history?.Value;
    public bool SuspendedEditing => _suspendedEditing?.Value ?? false;
    public PluginSetting<double> TreeWidth => _settingsService.GetSetting("ProfileEditor.TreeWidth", 350.0);
    public PluginSetting<double> ConditionsHeight => _settingsService.GetSetting("ProfileEditor.ConditionsHeight", 300.0);
    public PluginSetting<double> PropertiesHeight => _settingsService.GetSetting("ProfileEditor.PropertiesHeight", 300.0);
    public ReactiveCommand<Unit, Unit> ToggleSuspend { get; }
    public ReactiveCommand<Unit, Unit> ToggleAutoSuspend { get; }

    private void ExecuteToggleSuspend()
    {
        _profileEditorService.ChangeSuspendedEditing(!SuspendedEditing);
    }

    private void ExecuteToggleAutoSuspend()
    {
        PluginSetting<bool> setting = _settingsService.GetSetting("ProfileEditor.AutoSuspend", true);
        setting.Value = !setting.Value;
        setting.Save();
    }

    private void OnToolSelected(IChangeSet<IToolViewModel> changeSet)
    {
        IToolViewModel? changed = changeSet.FirstOrDefault()?.Item.Current;
        if (changed == null)
            return;

        if (!changed.IsSelected || !changed.IsExclusive)
            return;

        // Disable all others if the changed one is selected and exclusive
        _tools.Edit(list =>
        {
            foreach (IToolViewModel toolViewModel in list.Where(t => t.IsExclusive && t != changed))
                toolViewModel.IsSelected = false;
        });
    }

    private void InputServiceOnKeyboardKeyDown(object? sender, ArtemisKeyboardKeyEventArgs e)
    {
        if (!Shared.UI.KeyBindingsEnabled || !_mainWindowService.IsMainWindowFocused)
            return;

        if (e.Modifiers == KeyboardModifierKey.Control && e.Key == KeyboardKey.Z)
            History?.Undo.Execute().Subscribe();
        else if (e.Modifiers == KeyboardModifierKey.Control && e.Key == KeyboardKey.Y)
            History?.Redo.Execute().Subscribe();
        else if (e.Modifiers == KeyboardModifierKey.None && e.Key == KeyboardKey.F5)
            ToggleSuspend.Execute().Subscribe();
        else if (e.Modifiers == KeyboardModifierKey.Shift && e.Key == KeyboardKey.F5)
            ToggleAutoSuspend.Execute().Subscribe();
        else if (e.Modifiers == KeyboardModifierKey.None && e.Key == KeyboardKey.Space)
            PropertiesViewModel?.PlaybackViewModel.TogglePlay.Execute().Subscribe();
        else if (e.Modifiers == KeyboardModifierKey.Shift && e.Key == KeyboardKey.Space)
            PropertiesViewModel?.PlaybackViewModel.PlayFromStart.Execute().Subscribe();
        else if (e.Modifiers == KeyboardModifierKey.None && e.Key == KeyboardKey.F)
            (TitleBarViewModel as ProfileEditorTitleBarViewModel)?.MenuBarViewModel.CycleFocusMode.Execute().Subscribe();
        else
        {
            IToolViewModel? tool = Tools.FirstOrDefault(t => t.Hotkey != null && t.Hotkey.MatchesEventArgs(e));
            if (tool != null)
                tool.IsSelected = true;
        }
    }

    private void MainWindowServiceOnMainWindowFocused(object? sender, EventArgs e)
    {
        if (_settingsService.GetSetting("ProfileEditor.AutoSuspend", true).Value)
            _profileEditorService.ChangeSuspendedEditing(false);
    }

    private void MainWindowServiceOnMainWindowUnfocused(object? sender, EventArgs e)
    {
        if (_settingsService.GetSetting("ProfileEditor.AutoSuspend", true).Value)
            _profileEditorService.ChangeSuspendedEditing(true);
    }

    public ViewModelBase? TitleBarViewModel { get; }

    #region Overrides of RoutableScreen<object,ProfileEditorViewModelParameters>

    /// <inheritdoc />
    public override async Task OnNavigating(ProfileEditorViewModelParameters parameters, NavigationArguments args, CancellationToken cancellationToken)
    {
        ProfileConfiguration? profileConfiguration = _profileService.ProfileCategories.SelectMany(c => c.ProfileConfigurations).FirstOrDefault(c => c.ProfileId == parameters.ProfileId);

        // If the profile doesn't exist, cancel navigation
        if (profileConfiguration == null)
        {
            args.Cancel();
            return;
        }

        // If the profile is from the workshop, warn the user that auto-updates will be disabled
        InstalledEntry? workshopEntry = _workshopService.GetInstalledEntryByProfile(profileConfiguration);
        if (workshopEntry != null && workshopEntry.AutoUpdate)
        {
            bool confirmed = await _windowService.ShowConfirmContentDialog(
                "Editing a workshop profile",
                "You are about to edit a profile from the workshop, to preserve your changes auto-updating will be disabled.",
                "Disable auto-update");
            if (confirmed)
                _workshopService.SetAutoUpdate(workshopEntry, false);
            else
            {
                args.Cancel();
                return;
            }
        }

        await _profileEditorService.ChangeCurrentProfileConfiguration(profileConfiguration);
        ProfileConfiguration = profileConfiguration;
    }

    /// <inheritdoc />
    public override async Task OnClosing(NavigationArguments args)
    {
        if (!args.Path.StartsWith("profile-editor"))
        {
            ProfileConfiguration = null;
            await _profileEditorService.ChangeCurrentProfileConfiguration(null);
        }
    }

    #endregion
}

public class ProfileEditorViewModelParameters
{
    public Guid ProfileId { get; set; }
}