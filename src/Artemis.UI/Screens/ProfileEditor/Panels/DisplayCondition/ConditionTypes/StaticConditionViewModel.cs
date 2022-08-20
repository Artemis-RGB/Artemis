using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Screens.VisualScripting;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.ProfileEditor.Commands;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.DisplayCondition.ConditionTypes;

public class StaticConditionViewModel : ActivatableViewModelBase
{
    private readonly IProfileEditorService _profileEditorService;
    private readonly StaticCondition _staticCondition;
    private readonly IWindowService _windowService;
    private ObservableAsPropertyHelper<int>? _selectedPlayMode;
    private ObservableAsPropertyHelper<int>? _selectedStopMode;

    public StaticConditionViewModel(StaticCondition staticCondition, IProfileEditorService profileEditorService, IWindowService windowService)
    {
        _staticCondition = staticCondition;
        _profileEditorService = profileEditorService;
        _windowService = windowService;

        this.WhenActivated(d =>
        {
            _selectedPlayMode = staticCondition.WhenAnyValue(c => c.PlayMode).Select(m => (int) m).ToProperty(this, vm => vm.SelectedPlayMode).DisposeWith(d);
            _selectedStopMode = staticCondition.WhenAnyValue(c => c.StopMode).Select(m => (int) m).ToProperty(this, vm => vm.SelectedStopMode).DisposeWith(d);
        });

        OpenEditor = ReactiveCommand.CreateFromTask(ExecuteOpenEditor);
    }

    public ReactiveCommand<Unit, Unit> OpenEditor { get; }

    public int SelectedPlayMode
    {
        get => _selectedPlayMode?.Value ?? 0;
        set => _profileEditorService.ExecuteCommand(new UpdateStaticPlayMode(_staticCondition, (StaticPlayMode) value));
    }

    public int SelectedStopMode
    {
        get => _selectedStopMode?.Value ?? 0;
        set => _profileEditorService.ExecuteCommand(new UpdateStaticStopMode(_staticCondition, (StaticStopMode) value));
    }

    private async Task ExecuteOpenEditor()
    {
        await _windowService.ShowDialogAsync<NodeScriptWindowViewModel, bool>(("nodeScript", _staticCondition.Script));
        await _profileEditorService.SaveProfileAsync();
    }
}