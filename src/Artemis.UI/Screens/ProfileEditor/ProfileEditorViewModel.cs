using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Screens.ProfileEditor.DisplayCondition;
using Artemis.UI.Screens.ProfileEditor.ProfileTree;
using Artemis.UI.Screens.ProfileEditor.Properties;
using Artemis.UI.Screens.ProfileEditor.StatusBar;
using Artemis.UI.Screens.ProfileEditor.VisualEditor;
using Artemis.UI.Shared.Services.ProfileEditor;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor;

public class ProfileEditorViewModel : MainScreenViewModel
{
    private readonly IProfileEditorService _profileEditorService;
    private readonly ISettingsService _settingsService;
    private ObservableAsPropertyHelper<ProfileEditorHistory?>? _history;
    private ObservableAsPropertyHelper<ProfileConfiguration?>? _profileConfiguration;
    private ObservableAsPropertyHelper<bool>? _suspendedEditing;
    private ReadOnlyObservableCollection<IToolViewModel>? _tools;

    /// <inheritdoc />
    public ProfileEditorViewModel(IScreen hostScreen,
        IProfileEditorService profileEditorService,
        ISettingsService settingsService,
        VisualEditorViewModel visualEditorViewModel,
        ProfileTreeViewModel profileTreeViewModel,
        ProfileEditorTitleBarViewModel profileEditorTitleBarViewModel,
        PropertiesViewModel propertiesViewModel,
        DisplayConditionScriptViewModel displayConditionScriptViewModel,
        StatusBarViewModel statusBarViewModel)
        : base(hostScreen, "profile-editor")
    {
        _profileEditorService = profileEditorService;
        _settingsService = settingsService;
        VisualEditorViewModel = visualEditorViewModel;
        ProfileTreeViewModel = profileTreeViewModel;
        PropertiesViewModel = propertiesViewModel;
        DisplayConditionScriptViewModel = displayConditionScriptViewModel;
        StatusBarViewModel = statusBarViewModel;
        TitleBarViewModel = profileEditorTitleBarViewModel;

        this.WhenActivated(d =>
        {
            _profileConfiguration = profileEditorService.ProfileConfiguration.ToProperty(this, vm => vm.ProfileConfiguration).DisposeWith(d);
            _history = profileEditorService.History.ToProperty(this, vm => vm.History).DisposeWith(d);
            _suspendedEditing = profileEditorService.SuspendedEditing.ToProperty(this, vm => vm.SuspendedEditing).DisposeWith(d);
            profileEditorService.Tools.Connect()
                .Filter(t => t.ShowInToolbar)
                .Sort(SortExpressionComparer<IToolViewModel>.Ascending(vm => vm.Order))
                .Bind(out ReadOnlyObservableCollection<IToolViewModel> tools)
                .Subscribe()
                .DisposeWith(d);
            Tools = tools;
        });

        ToggleSuspend = ReactiveCommand.Create(ExecuteToggleSuspend);
        ToggleAutoSuspend = ReactiveCommand.Create(ExecuteToggleAutoSuspend);
    }

    public VisualEditorViewModel VisualEditorViewModel { get; }
    public ProfileTreeViewModel ProfileTreeViewModel { get; }
    public PropertiesViewModel PropertiesViewModel { get; }
    public DisplayConditionScriptViewModel DisplayConditionScriptViewModel { get; }
    public StatusBarViewModel StatusBarViewModel { get; }

    public ReadOnlyObservableCollection<IToolViewModel>? Tools
    {
        get => _tools;
        set => RaiseAndSetIfChanged(ref _tools, value);
    }

    public ProfileConfiguration? ProfileConfiguration => _profileConfiguration?.Value;
    public ProfileEditorHistory? History => _history?.Value;
    public bool SuspendedEditing => _suspendedEditing?.Value ?? false;
    public PluginSetting<double> TreeWidth => _settingsService.GetSetting("ProfileEditor.TreeWidth", 350.0);
    public PluginSetting<double> ConditionsHeight => _settingsService.GetSetting("ProfileEditor.ConditionsHeight", 300.0);
    public PluginSetting<double> PropertiesHeight => _settingsService.GetSetting("ProfileEditor.PropertiesHeight", 300.0);
    public ReactiveCommand<Unit, Unit> ToggleSuspend { get; }
    public ReactiveCommand<Unit, Unit> ToggleAutoSuspend { get; }

    public void OpenUrl(string url)
    {
        Utilities.OpenUrl(url);
    }

    private void ExecuteToggleSuspend()
    {
        _profileEditorService.ChangeSuspendedEditing(!SuspendedEditing);
    }

    private void ExecuteToggleAutoSuspend()
    {
    }
}