using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared.Services.NodeEditor;

namespace Artemis.UI.Screens.VisualScripting.Pins;

public class OutputPinCollectionViewModel<T> : PinCollectionViewModel
{
    private readonly INodeVmFactory _nodeVmFactory;
    public OutputPinCollection<T> OutputPinCollection { get; }

    public OutputPinCollectionViewModel(OutputPinCollection<T> outputPinCollection, NodeScriptViewModel nodeScriptViewModel, INodeVmFactory nodeVmFactory, INodeEditorService nodeEditorService)
        : base(outputPinCollection, nodeScriptViewModel, nodeEditorService)
    {
        _nodeVmFactory = nodeVmFactory;
        OutputPinCollection = outputPinCollection;
    }

    protected override PinViewModel CreatePinViewModel(IPin pin)
    {
        PinViewModel vm = _nodeVmFactory.OutputPinViewModel(pin);
        vm.RemovePin = RemovePin;
        return vm;
    }
}