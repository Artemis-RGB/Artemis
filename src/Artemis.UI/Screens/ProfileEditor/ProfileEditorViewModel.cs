using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
using Avalonia.Threading;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor;

public class ProfileEditorViewModel : MainScreenViewModel
{
    private readonly IProfileEditorService _profileEditorService;
    private readonly ISettingsService _settingsService;
    private readonly SourceList<IToolViewModel> _tools;
    private ObservableAsPropertyHelper<ProfileEditorHistory?>? _history;
    private ObservableAsPropertyHelper<ProfileConfiguration?>? _profileConfiguration;
    private ObservableAsPropertyHelper<bool>? _suspendedEditing;
    private StatusBarViewModel? _statusBarViewModel;
    private DisplayConditionScriptViewModel? _displayConditionScriptViewModel;
    private PropertiesViewModel? _propertiesViewModel;
    private ProfileTreeViewModel? _profileTreeViewModel;
    private VisualEditorViewModel? _visualEditorViewModel;

    /// <inheritdoc />
    public ProfileEditorViewModel(IScreen hostScreen,
        IProfileEditorService profileEditorService,
        ISettingsService settingsService,
        VisualEditorViewModel visualEditorViewModel,
        ProfileTreeViewModel profileTreeViewModel,
        ProfileEditorTitleBarViewModel profileEditorTitleBarViewModel,
        PropertiesViewModel propertiesViewModel,
        DisplayConditionScriptViewModel displayConditionScriptViewModel,
        StatusBarViewModel statusBarViewModel,
        IEnumerable<IToolViewModel> toolViewModels)
        : base(hostScreen, "profile-editor")
    {
        _profileEditorService = profileEditorService;
        _settingsService = settingsService;

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

        this.WhenActivated(d =>
        {
            _profileConfiguration = profileEditorService.ProfileConfiguration.ToProperty(this, vm => vm.ProfileConfiguration).DisposeWith(d);
            _history = profileEditorService.History.ToProperty(this, vm => vm.History).DisposeWith(d);
            _suspendedEditing = profileEditorService.SuspendedEditing.ToProperty(this, vm => vm.SuspendedEditing).DisposeWith(d);

            // Slow and steady wins the race (and doesn't lock up the entire UI)
            Dispatcher.UIThread.Post(() => StatusBarViewModel = statusBarViewModel, DispatcherPriority.Loaded);
            Dispatcher.UIThread.Post(() => VisualEditorViewModel = visualEditorViewModel, DispatcherPriority.Loaded);
            Dispatcher.UIThread.Post(() => ProfileTreeViewModel = profileTreeViewModel, DispatcherPriority.Loaded);
            Dispatcher.UIThread.Post(() => PropertiesViewModel = propertiesViewModel, DispatcherPriority.Loaded);
            Dispatcher.UIThread.Post(() => DisplayConditionScriptViewModel = displayConditionScriptViewModel, DispatcherPriority.Loaded);
        });

        TitleBarViewModel = profileEditorTitleBarViewModel;
        ToggleSuspend = ReactiveCommand.Create(ExecuteToggleSuspend);
        ToggleAutoSuspend = ReactiveCommand.Create(ExecuteToggleAutoSuspend);
    }

    public VisualEditorViewModel? VisualEditorViewModel
    {
        get => _visualEditorViewModel;
        set => RaiseAndSetIfChanged(ref _visualEditorViewModel, value);
    }

    public ProfileTreeViewModel? ProfileTreeViewModel
    {
        get => _profileTreeViewModel;
        set => RaiseAndSetIfChanged(ref _profileTreeViewModel, value);
    }

    public PropertiesViewModel? PropertiesViewModel
    {
        get => _propertiesViewModel;
        set => RaiseAndSetIfChanged(ref _propertiesViewModel, value);
    }

    public DisplayConditionScriptViewModel? DisplayConditionScriptViewModel
    {
        get => _displayConditionScriptViewModel;
        set => RaiseAndSetIfChanged(ref _displayConditionScriptViewModel, value);
    }

    public StatusBarViewModel? StatusBarViewModel
    {
        get => _statusBarViewModel;
        set => RaiseAndSetIfChanged(ref _statusBarViewModel, value);
    }

    public ReadOnlyObservableCollection<IToolViewModel> Tools { get; }
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
}