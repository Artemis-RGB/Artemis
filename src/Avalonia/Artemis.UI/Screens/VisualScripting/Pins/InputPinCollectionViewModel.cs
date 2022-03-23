using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared.Services.NodeEditor;

namespace Artemis.UI.Screens.VisualScripting.Pins;

public class InputPinCollectionViewModel<T> : PinCollectionViewModel
{
    private readonly INodeVmFactory _nodeVmFactory;
    public InputPinCollection<T> InputPinCollection { get; }

    public InputPinCollectionViewModel(InputPinCollection<T> inputPinCollection, NodeScriptViewModel nodeScriptViewModel, INodeVmFactory nodeVmFactory, INodeEditorService nodeEditorService)
        : base(inputPinCollection, nodeScriptViewModel, nodeEditorService)
    {
        _nodeVmFactory = nodeVmFactory;
        InputPinCollection = inputPinCollection;
    }
    
    protected override PinViewModel CreatePinViewModel(IPin pin)
    {
        PinViewModel vm = _nodeVmFactory.InputPinViewModel(pin);
        vm.RemovePin = RemovePin;
        return vm;
    }
}