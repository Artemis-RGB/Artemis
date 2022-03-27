using System.Collections.ObjectModel;
using Artemis.Core;
using Artemis.UI.Shared.Services.NodeEditor;
using Artemis.UI.Shared.Services.NodeEditor.Commands;
using Artemis.UI.Shared.VisualScripting;
using ReactiveUI;

namespace Artemis.VisualScripting.Nodes.Easing.CustomViewModels;

public class EasingTypeNodeCustomViewModel : CustomNodeViewModel
{
    private readonly EasingTypeNode _node;
    private readonly INodeEditorService _nodeEditorService;

    public EasingTypeNodeCustomViewModel(EasingTypeNode node, INodeScript script, INodeEditorService nodeEditorService) : base(node, script)
    {
        _node = node;
        _nodeEditorService = nodeEditorService;

        NodeModified += (_, _) => this.RaisePropertyChanged(nameof(SelectedEasingViewModel));
        EasingViewModels = new ObservableCollection<NodeEasingViewModel>(Enum.GetValues(typeof(Easings.Functions)).Cast<Easings.Functions>().Select(e => new NodeEasingViewModel(e)));
    }

    public ObservableCollection<NodeEasingViewModel> EasingViewModels { get; }

    public NodeEasingViewModel? SelectedEasingViewModel
    {
        get => EasingViewModels.FirstOrDefault(e => e.EasingFunction == _node.Storage);
        set
        {
            if (value != null && _node.Storage != value.EasingFunction)
                _nodeEditorService.ExecuteCommand(Script, new UpdateStorage<Easings.Functions>(_node, value.EasingFunction));
        }
    }
}