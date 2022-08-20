using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared.Services.NodeEditor;

namespace Artemis.UI.Screens.VisualScripting.Pins;

public class InputPinCollectionViewModel : PinCollectionViewModel
{
    private readonly NodeScriptViewModel _nodeScriptViewModel;
    private readonly INodeVmFactory _nodeVmFactory;
    public IPinCollection InputPinCollection { get; }

    public InputPinCollectionViewModel(IPinCollection inputPinCollection, NodeScriptViewModel nodeScriptViewModel, INodeVmFactory nodeVmFactory, INodeEditorService nodeEditorService)
        : base(inputPinCollection, nodeScriptViewModel, nodeEditorService)
    {
        _nodeScriptViewModel = nodeScriptViewModel;
        _nodeVmFactory = nodeVmFactory;
        InputPinCollection = inputPinCollection;
    }
    
    protected override PinViewModel CreatePinViewModel(IPin pin)
    {
        PinViewModel vm = _nodeVmFactory.InputPinViewModel(pin, _nodeScriptViewModel);
        vm.RemovePin = RemovePin;
        return vm;
    }
}