using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared.Services.NodeEditor;

namespace Artemis.UI.Screens.VisualScripting.Pins;

public class InputPinCollectionViewModel<T> : PinCollectionViewModel
{
    public InputPinCollection<T> InputPinCollection { get; }

    public InputPinCollectionViewModel(InputPinCollection<T> inputPinCollection, NodeScriptViewModel nodeScriptViewModel, INodePinVmFactory nodePinVmFactory, INodeEditorService nodeEditorService) 
        : base(inputPinCollection, nodeScriptViewModel, nodePinVmFactory, nodeEditorService)
    {
        InputPinCollection = inputPinCollection;
    }
}