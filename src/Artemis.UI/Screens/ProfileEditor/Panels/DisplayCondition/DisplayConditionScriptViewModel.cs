using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.VisualScripting;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.ProfileEditor.Commands;
using Avalonia.Controls.Mixins;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.DisplayCondition;

public class DisplayConditionScriptViewModel : ActivatableViewModelBase
{
    private readonly IProfileEditorService _profileEditorService;
    private readonly ObservableAsPropertyHelper<bool> _showEventOptions;
    private readonly ObservableAsPropertyHelper<bool> _showStaticOptions;
    private readonly IWindowService _windowService;
    private ObservableAsPropertyHelper<NodeScriptViewModel?>? _nodeScriptViewModel;
    private RenderProfileElement? _profileElement;
    private ObservableAsPropertyHelper<ConditionTypeViewModel?>? _selectedConditionTypeViewModel;

    public DisplayConditionScriptViewModel(IProfileEditorService profileEditorService, INodeVmFactory nodeVmFactory, IWindowService windowService)
    {
        _profileEditorService = profileEditorService;
        _windowService = windowService;

        ConditionTypeViewModels = new ObservableCollection<ConditionTypeViewModel>
        {
            new("None", "The element is always active.", null),
            new("Regular", "The element is activated when the provided visual script ends in true.", typeof(StaticCondition)),
            new("Event", "The element is activated when the selected event fires.\r\n" +
                         "Events that contain data can conditionally trigger the layer using a visual script.", typeof(EventCondition))
        };

        this.WhenActivated(d =>
        {
            profileEditorService.ProfileElement.Subscribe(p => _profileElement = p).DisposeWith(d);

            _nodeScriptViewModel = profileEditorService.ProfileElement
                .Select(p => p?.WhenAnyValue(element => element.DisplayCondition) ?? Observable.Never<ICondition?>())
                .Switch()
                .Select(c => c is INodeScriptCondition {NodeScript: NodeScript nodeScript} ? nodeVmFactory.NodeScriptViewModel(nodeScript, true) : null)
                .ToProperty(this, vm => vm.NodeScriptViewModel)
                .DisposeWith(d);
            _selectedConditionTypeViewModel = profileEditorService.ProfileElement
                .Select(p => p?.WhenAnyValue(element => element.DisplayCondition) ?? Observable.Never<ICondition?>())
                .Switch()
                .Select(c => c != null ? ConditionTypeViewModels.FirstOrDefault(vm => vm.ConditionType == c.GetType()) : null)
                .ToProperty(this, vm => vm.SelectedConditionTypeViewModel)
                .DisposeWith(d);
        });

        _showStaticOptions = this.WhenAnyValue(vm => vm.SelectedConditionTypeViewModel)
            .Select(c => c != null && c.ConditionType == typeof(StaticCondition))
            .ToProperty(this, vm => vm.ShowStaticOptions);
        _showEventOptions = this.WhenAnyValue(vm => vm.SelectedConditionTypeViewModel)
            .Select(c => c != null && c.ConditionType == typeof(EventCondition))
            .ToProperty(this, vm => vm.ShowEventOptions);
    }

    public NodeScriptViewModel? NodeScriptViewModel => _nodeScriptViewModel?.Value;
    public ObservableCollection<ConditionTypeViewModel> ConditionTypeViewModels { get; }

    public ConditionTypeViewModel? SelectedConditionTypeViewModel
    {
        get => _selectedConditionTypeViewModel?.Value;
        set
        {
            if (_profileElement == null)
                return;

            ICondition? condition = null;
            if (value?.ConditionType == typeof(StaticCondition))
                condition = new StaticCondition(_profileElement);
            else if (value?.ConditionType == typeof(EventCondition))
                condition = new EventCondition(_profileElement);

            _profileEditorService.ExecuteCommand(new ChangeConditionType(_profileElement, condition));
        }
    }

    public bool ShowStaticOptions => _showStaticOptions.Value;
    public bool ShowEventOptions => _showEventOptions.Value;


    public async Task OpenEditor()
    {
        if (_profileElement?.DisplayCondition is StaticCondition staticCondition)
            await _windowService.ShowDialogAsync<NodeScriptWindowViewModel, bool>(("nodeScript", staticCondition.Script));
    }
}