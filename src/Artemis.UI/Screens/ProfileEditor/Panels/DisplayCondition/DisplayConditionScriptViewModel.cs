using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.VisualScripting;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.Interfaces;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.ProfileEditor.Commands;
using Avalonia.Controls.Mixins;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.DisplayCondition;

public class DisplayConditionScriptViewModel : ActivatableViewModelBase
{
    private readonly IProfileEditorService _profileEditorService;
    private readonly IWindowService _windowService;
    private ObservableAsPropertyHelper<NodeScriptViewModel?>? _nodeScriptViewModel;
    private RenderProfileElement? _profileElement;

    public DisplayConditionScriptViewModel(IProfileEditorService profileEditorService, INodeVmFactory nodeVmFactory, IWindowService windowService)
    {
        _profileEditorService = profileEditorService;
        _windowService = windowService;
        this.WhenActivated(d =>
        {
            profileEditorService.ProfileElement.Subscribe(p => _profileElement = p).DisposeWith(d);
            _nodeScriptViewModel = profileEditorService.ProfileElement
                .Select(p => p?.WhenAnyValue(element => element.DisplayCondition) ?? Observable.Never<ICondition?>())
                .Switch()
                .Select(c => c is StaticCondition staticCondition ? nodeVmFactory.NodeScriptViewModel(staticCondition.Script, true) : null)
                .ToProperty(this, vm => vm.NodeScriptViewModel)
                .DisposeWith(d);
        });
    }

    public NodeScriptViewModel? NodeScriptViewModel => _nodeScriptViewModel?.Value;

    public async Task EnableConditions()
    {
        bool confirmed = await _windowService.ShowConfirmContentDialog(
            "Display conditions",
            "Do you want to enable display conditions for this element? \r\n" +
            "Using display conditions you can dynamically hide or show layers and folders depending on certain parameters."
        );

        if (confirmed && _profileElement != null)
            _profileEditorService.ExecuteCommand(new ChangeElementDisplayCondition(_profileElement, new StaticCondition(_profileElement)));
    }

    public async Task OpenEditor()
    {
        if (_profileElement?.DisplayCondition is StaticCondition staticCondition)
            await _windowService.ShowDialogAsync<NodeScriptWindowViewModel, bool>(("nodeScript", staticCondition.Script));
    }
}