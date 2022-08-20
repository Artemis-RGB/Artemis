using System.Collections.ObjectModel;
using Artemis.Core;
using Artemis.UI.Shared.Services.NodeEditor;
using Artemis.UI.Shared.Services.NodeEditor.Commands;
using Artemis.UI.Shared.VisualScripting;
using ReactiveUI;

namespace Artemis.VisualScripting.Nodes.Color.Screens;

public class QuantizeNodeCustomViewModel : CustomNodeViewModel
{
    #region Properties & Fields

    private readonly QuantizeNode _node;
    private readonly INodeEditorService _nodeEditorService;

    public ObservableCollection<int> PaletteSizes { get; } = new() { 1, 2, 4, 8, 16, 32, 64, 128, 256, 512 };

    public int PaletteSize
    {
        get => _node.Storage?.PaletteSize ?? 32;
        set
        {
            if ((_node.Storage != null) && (_node.Storage.PaletteSize != value))
            {
                _node.Storage.PaletteSize = value;
                _nodeEditorService.ExecuteCommand(Script, new UpdateStorage<QuantizeNodeStorage>(_node, _node.Storage));
            }
        }
    }

    public bool IgnoreLimits
    {
        get => _node.Storage?.IgnoreLimits ?? false;
        set
        {
            if ((_node.Storage != null) && (_node.Storage.IgnoreLimits != value))
            {
                _node.Storage.IgnoreLimits = value;
                _nodeEditorService.ExecuteCommand(Script, new UpdateStorage<QuantizeNodeStorage>(_node, _node.Storage));
            }
        }
    }

    #endregion

    #region Constructors

    public QuantizeNodeCustomViewModel(QuantizeNode node, INodeScript script, INodeEditorService nodeEditorService) : base(node, script)
    {
        this._node = node;
        this._nodeEditorService = nodeEditorService;

        NodeModified += (_, _) => this.RaisePropertyChanged(nameof(PaletteSize));
    }

    #endregion
}