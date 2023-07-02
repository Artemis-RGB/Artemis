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
using Artemis.UI.Screens.ProfileEditor.DisplayCondition;
using Artemis.UI.Screens.ProfileEditor.ProfileTree;
using Artemis.UI.Screens.ProfileEditor.Properties;
using Artemis.UI.Screens.ProfileEditor.StatusBar;
using Artemis.UI.Screens.ProfileEditor.VisualEditor;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services.MainWindow;
using Artemis.UI.Shared.Services.ProfileEditor;
using Avalonia.Threading;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor;

public class ProfileEditorViewModel : RoutableScreen<object, ProfileEditorViewModelParameters>, IMainScreenViewModel
{
    private readonly IProfileEditorService _profileEditorService;
    private readonly IProfileService _profileService;
    private readonly ISettingsService _settingsService;
    private readonly SourceList<IToolViewModel> _tools;
    private ObservableAsPropertyHelper<ProfileEditorHistory?>? _history;
    private ProfileConfiguration? _profileConfiguration;
    private ObservableAsPropertyHelper<bool>? _suspendedEditing;

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
        IMainWindowService mainWindowService)
    {
        _profileService = profileService;
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

        StatusBarViewModel = statusBarViewModel;
        VisualEditorViewModel = visualEditorViewModel;
        ProfileTreeViewModel = profileTreeViewModel;
        PropertiesViewModel = propertiesViewModel;
        DisplayConditionScriptViewModel = displayConditionScriptViewModel;
        
        this.WhenActivated(d =>
        {
            _history = profileEditorService.History.ToProperty(this, vm => vm.History).DisposeWith(d);
            _suspendedEditing = profileEditorService.SuspendedEditing.ToProperty(this, vm => vm.SuspendedEditing).DisposeWith(d);

            mainWindowService.MainWindowFocused += MainWindowServiceOnMainWindowFocused;
            mainWindowService.MainWindowUnfocused += MainWindowServiceOnMainWindowUnfocused;

            Disposable.Create(() =>
            {
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

    public ProfileConfiguration? ProfileConfiguration
    {
        get => _profileConfiguration;
        set => RaiseAndSetIfChanged(ref _profileConfiguration, value);
    }

    public VisualEditorViewModel? VisualEditorViewModel { get; }
    public ProfileTreeViewModel? ProfileTreeViewModel{ get; }
    public PropertiesViewModel? PropertiesViewModel{ get; }
    public DisplayConditionScriptViewModel? DisplayConditionScriptViewModel{ get; }
    public StatusBarViewModel? StatusBarViewModel{ get; }

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
    public override Task OnNavigating(ProfileEditorViewModelParameters parameters, NavigationArguments args, CancellationToken cancellationToken)
    {
        ProfileConfiguration? profileConfiguration = _profileService.ProfileConfigurations.FirstOrDefault(c => c.ProfileId == parameters.ProfileId);
        ProfileConfiguration = profileConfiguration;
        _profileEditorService.ChangeCurrentProfileConfiguration(profileConfiguration);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override Task OnClosing(NavigationArguments args)
    {
        if (!args.Path.StartsWith("profile-editor"))
            _profileEditorService.ChangeCurrentProfileConfiguration(null);
        return base.OnClosing(args);
    }

    #endregion
}

public class ProfileEditorViewModelParameters
{
    public Guid ProfileId { get; set; }
}