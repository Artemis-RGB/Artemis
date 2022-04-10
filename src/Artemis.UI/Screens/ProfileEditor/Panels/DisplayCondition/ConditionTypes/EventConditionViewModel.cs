using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Screens.VisualScripting;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.ProfileEditor.Commands;
using Avalonia.Controls.Mixins;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.DisplayCondition.ConditionTypes;

public class EventConditionViewModel : ActivatableViewModelBase
{
    private readonly EventCondition _eventCondition;
    private readonly IProfileEditorService _profileEditorService;
    private readonly ObservableAsPropertyHelper<bool> _showOverlapOptions;
    private readonly IWindowService _windowService;
    private readonly ISettingsService _settingsService;
    private ObservableAsPropertyHelper<DataModelPath?>? _eventPath;
    private ObservableAsPropertyHelper<int>? _selectedOverlapMode;
    private ObservableAsPropertyHelper<int>? _selectedTriggerMode;

    public EventConditionViewModel(EventCondition eventCondition, IProfileEditorService profileEditorService, IWindowService windowService, ISettingsService settingsService)
    {
        _eventCondition = eventCondition;
        _profileEditorService = profileEditorService;
        _windowService = windowService;
        _settingsService = settingsService;
        _showOverlapOptions = this.WhenAnyValue(vm => vm.SelectedTriggerMode)
            .Select(m => m == 0)
            .ToProperty(this, vm => vm.ShowOverlapOptions);

        this.WhenActivated(d =>
        {
            _eventPath = eventCondition.WhenAnyValue(c => c.EventPath).ToProperty(this, vm => vm.EventPath).DisposeWith(d);
            _selectedTriggerMode = eventCondition.WhenAnyValue(c => c.TriggerMode).Select(m => (int) m).ToProperty(this, vm => vm.SelectedTriggerMode).DisposeWith(d);
            _selectedOverlapMode = eventCondition.WhenAnyValue(c => c.OverlapMode).Select(m => (int) m).ToProperty(this, vm => vm.SelectedOverlapMode).DisposeWith(d);
        });

        OpenEditor = ReactiveCommand.CreateFromTask(ExecuteOpenEditor);
    }

    public ObservableCollection<Type> FilterTypes { get; } = new() {typeof(IDataModelEvent)};
    public ReactiveCommand<Unit, Unit> OpenEditor { get; }
    public bool ShowOverlapOptions => _showOverlapOptions.Value;
    public bool IsConditionForLayer => _eventCondition.ProfileElement is Layer;
    public PluginSetting<bool> ShowFullPaths => _settingsService.GetSetting("ProfileEditor.ShowFullPaths", false);

    public DataModelPath? EventPath
    {
        get => _eventPath?.Value != null ? new DataModelPath(_eventPath.Value) : null;
        set => _profileEditorService.ExecuteCommand(new UpdateEventConditionPath(_eventCondition, value != null ? new DataModelPath(value) : null));
    }

    public int SelectedTriggerMode
    {
        get => _selectedTriggerMode?.Value ?? 0;
        set => _profileEditorService.ExecuteCommand(new UpdateEventTriggerMode(_eventCondition, (EventTriggerMode) value));
    }

    public int SelectedOverlapMode
    {
        get => _selectedOverlapMode?.Value ?? 0;
        set => _profileEditorService.ExecuteCommand(new UpdateEventOverlapMode(_eventCondition, (EventOverlapMode) value));
    }
    
    private async Task ExecuteOpenEditor()
    {
        await _windowService.ShowDialogAsync<NodeScriptWindowViewModel, bool>(("nodeScript", _eventCondition.NodeScript));
    }
}