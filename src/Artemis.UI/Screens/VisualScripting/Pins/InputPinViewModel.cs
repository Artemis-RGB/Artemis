using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared.Services.NodeEditor;

namespace Artemis.UI.Screens.VisualScripting.Pins;

public class InputPinViewModel : PinViewModel
{
    public InputPinViewModel(IPin inputPin, NodeScriptViewModel nodeScriptViewModel, INodeService nodeService, INodeEditorService nodeEditorService)
        : base(inputPin, nodeScriptViewModel, nodeService, nodeEditorService)
    {
    }
}