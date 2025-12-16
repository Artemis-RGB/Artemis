using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.UI.DryIoc.Factories;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.ProfileEditor.Commands;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.DisplayCondition;

public class DisplayConditionScriptViewModel : ActivatableViewModelBase
{
    private readonly ObservableAsPropertyHelper<ViewModelBase?> _conditionViewModel;
    private readonly IConditionVmFactory _conditionVmFactory;
    private readonly IProfileEditorService _profileEditorService;
    private ObservableAsPropertyHelper<RenderProfileElement?>? _profileElement;
    private ObservableAsPropertyHelper<ConditionTypeViewModel?>? _selectedConditionTypeViewModel;

    public DisplayConditionScriptViewModel(IProfileEditorService profileEditorService, IConditionVmFactory conditionVmFactory)
    {
        _profileEditorService = profileEditorService;
        _conditionVmFactory = conditionVmFactory;

        ConditionTypeViewModels = new ObservableCollection<ConditionTypeViewModel>
        {
            new("Always", "The element is always active.", typeof(AlwaysOnCondition)),
            new("Once", "The element is shown once until its timeline is finished.", typeof(PlayOnceCondition)),
            new("Conditional", "The element is activated when the provided visual script ends in true.", typeof(StaticCondition)),
            new("On event", "The element is activated when the selected event fires.\r\n" +
                            "Events that contain data can conditionally trigger the layer using a visual script.", typeof(EventCondition))
        };

        this.WhenActivated(d =>
        {
            _profileElement = profileEditorService.ProfileElement.ToProperty(this, vm => vm.ProfileElement).DisposeWith(d);
            _selectedConditionTypeViewModel = profileEditorService.ProfileElement
                .Select(p => p?.WhenAnyValue(element => element.DisplayCondition) ?? Observable.Return<ICondition?>(null))
                .Switch()
                .Select(c => c != null ? ConditionTypeViewModels.FirstOrDefault(vm => vm.ConditionType == c.GetType()) : null)
                .ToProperty(this, vm => vm.SelectedConditionTypeViewModel)
                .DisposeWith(d);
        });

        _conditionViewModel = this.WhenAnyValue(vm => vm.SelectedConditionTypeViewModel, vm => vm.ProfileElement).Select(_ => CreateConditionViewModel()).ToProperty(this, vm => vm.ConditionViewModel);
    }

    public RenderProfileElement? ProfileElement => _profileElement?.Value;
    public ViewModelBase? ConditionViewModel => _conditionViewModel.Value;
    public ObservableCollection<ConditionTypeViewModel> ConditionTypeViewModels { get; }

    public ConditionTypeViewModel? SelectedConditionTypeViewModel
    {
        get => _selectedConditionTypeViewModel?.Value;
        set => ApplyConditionType(value);
    }

    private ViewModelBase? CreateConditionViewModel()
    {
        if (ProfileElement == null)
            return null;

        if (ProfileElement.DisplayCondition is AlwaysOnCondition alwaysOnCondition)
            return _conditionVmFactory.AlwaysOnConditionViewModel(alwaysOnCondition);
        if (ProfileElement.DisplayCondition is PlayOnceCondition playOnceCondition)
            return _conditionVmFactory.PlayOnceConditionViewModel(playOnceCondition);
        if (ProfileElement.DisplayCondition is StaticCondition staticCondition)
            return _conditionVmFactory.StaticConditionViewModel(staticCondition);
        if (ProfileElement.DisplayCondition is EventCondition eventCondition)
            return _conditionVmFactory.EventConditionViewModel(eventCondition);

        return null;
    }

    private void ApplyConditionType(ConditionTypeViewModel? value)
    {
        if (ProfileElement == null || value == null || ProfileElement.DisplayCondition.GetType() == value.ConditionType)
            return;

        if (value.ConditionType == typeof(AlwaysOnCondition))
            _profileEditorService.ExecuteCommand(new ChangeConditionType(ProfileElement, new AlwaysOnCondition(ProfileElement)));
        if (value.ConditionType == typeof(PlayOnceCondition))
            _profileEditorService.ExecuteCommand(new ChangeConditionType(ProfileElement, new PlayOnceCondition(ProfileElement)));
        if (value.ConditionType == typeof(StaticCondition))
            _profileEditorService.ExecuteCommand(new ChangeConditionType(ProfileElement, new StaticCondition(ProfileElement)));
        if (value.ConditionType == typeof(EventCondition))
            _profileEditorService.ExecuteCommand(new ChangeConditionType(ProfileElement, new EventCondition(ProfileElement)));
    }
}