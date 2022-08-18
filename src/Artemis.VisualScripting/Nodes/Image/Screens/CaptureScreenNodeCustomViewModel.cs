using Artemis.Core;
using Artemis.UI.Shared.Services.NodeEditor;
using Artemis.UI.Shared.VisualScripting;

namespace Artemis.VisualScripting.Nodes.Image.Screens;

public class CaptureScreenNodeCustomViewModel : CustomNodeViewModel
{
    #region Properties & Fields

    private readonly CaptureScreenNode _node;
    private readonly INodeEditorService _nodeEditorService;

    #endregion

    #region Constructors

    /// <inheritdoc />
    public CaptureScreenNodeCustomViewModel(CaptureScreenNode node, INodeScript script, INodeEditorService nodeEditorService) 
        : base(node, script)
    {
        this._node = node;
        this._nodeEditorService = nodeEditorService;
    }

    #endregion

    #region Methods
    
    #endregion
}