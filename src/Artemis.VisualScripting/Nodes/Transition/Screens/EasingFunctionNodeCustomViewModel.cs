using System.Collections.ObjectModel;
using Artemis.Core;
using Artemis.UI.Shared.Services.NodeEditor;
using Artemis.UI.Shared.Services.NodeEditor.Commands;
using Artemis.UI.Shared.VisualScripting;
using ReactiveUI;

namespace Artemis.VisualScripting.Nodes.Transition.Screens;

public class EasingFunctionNodeCustomViewModel : CustomNodeViewModel
{
    private readonly EasingFunctionNode _node;
    private readonly INodeEditorService _nodeEditorService;

    public EasingFunctionNodeCustomViewModel(EasingFunctionNode node, INodeScript script, INodeEditorService nodeEditorService) : base(node, script)
    {
        _node = node;
        _nodeEditorService = nodeEditorService;

        NodeModified += (_, _) => this.RaisePropertyChanged(nameof(SelectedEasingViewModel));
        EasingViewModels =
            new ObservableCollection<EasingFunctionViewModel>(Enum.GetValues(typeof(Easings.Functions)).Cast<Easings.Functions>().Select(e => new EasingFunctionViewModel(e)));
    }

    public ObservableCollection<EasingFunctionViewModel> EasingViewModels { get; }

    public EasingFunctionViewModel? SelectedEasingViewModel
    {
        get => EasingViewModels.FirstOrDefault(e => e.EasingFunction == _node.Storage);
        set
        {
            if (value != null && _node.Storage != value.EasingFunction)
                _nodeEditorService.ExecuteCommand(Script, new UpdateStorage<Easings.Functions>(_node, value.EasingFunction));
        }
    }
}