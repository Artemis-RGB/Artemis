using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared.Services.NodeEditor;

namespace Artemis.UI.Screens.VisualScripting.Pins;

public class OutputPinViewModel : PinViewModel
{
    public OutputPinViewModel(IPin outputPin, NodeScriptViewModel nodeScriptViewModel, INodeService nodeService, INodeEditorService nodeEditorService) 
        : base(outputPin, nodeScriptViewModel, nodeService, nodeEditorService)
    {
    }
}