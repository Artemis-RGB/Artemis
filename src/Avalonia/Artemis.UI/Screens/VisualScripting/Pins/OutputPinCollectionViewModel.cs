using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared.Services.NodeEditor;

namespace Artemis.UI.Screens.VisualScripting.Pins;

public class OutputPinCollectionViewModel<T> : PinCollectionViewModel
{
    public OutputPinCollection<T> OutputPinCollection { get; }

    public OutputPinCollectionViewModel(OutputPinCollection<T> outputPinCollection, NodeScriptViewModel nodeScriptViewModel, INodePinVmFactory nodePinVmFactory, INodeEditorService nodeEditorService)
        : base(outputPinCollection, nodeScriptViewModel, nodePinVmFactory, nodeEditorService)
    {
        OutputPinCollection = outputPinCollection;
    }
}