using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared.Services.NodeEditor;

namespace Artemis.UI.Screens.VisualScripting.Pins;

public class OutputPinCollectionViewModel : PinCollectionViewModel
{
    private readonly INodeVmFactory _nodeVmFactory;
    public IPinCollection OutputPinCollection { get; }

    public OutputPinCollectionViewModel(IPinCollection outputPinCollection, NodeScriptViewModel nodeScriptViewModel, INodeVmFactory nodeVmFactory, INodeEditorService nodeEditorService)
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