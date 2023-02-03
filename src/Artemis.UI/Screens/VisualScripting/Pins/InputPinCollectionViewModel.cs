using Artemis.Core;
using Artemis.UI.DryIoc.Factories;
using Artemis.UI.Shared.Services.NodeEditor;

namespace Artemis.UI.Screens.VisualScripting.Pins;

public class InputPinCollectionViewModel : PinCollectionViewModel
{
    private readonly NodeScriptViewModel _nodeScriptViewModel;
    private readonly INodeVmFactory _nodeVmFactory;

    public InputPinCollectionViewModel(IPinCollection inputPinCollection, NodeScriptViewModel nodeScriptViewModel, INodeVmFactory nodeVmFactory, INodeEditorService nodeEditorService)
        : base(inputPinCollection, nodeScriptViewModel, nodeEditorService)
    {
        _nodeScriptViewModel = nodeScriptViewModel;
        _nodeVmFactory = nodeVmFactory;
        InputPinCollection = inputPinCollection;
    }

    public IPinCollection InputPinCollection { get; }

    protected override PinViewModel CreatePinViewModel(IPin pin)
    {
        PinViewModel vm = _nodeVmFactory.InputPinViewModel(pin, _nodeScriptViewModel);
        vm.RemovePin = RemovePin;
        return vm;
    }
}