using Artemis.Core;
using Artemis.UI.DryIoc.Factories;
using Artemis.UI.Shared.Services.NodeEditor;

namespace Artemis.UI.Screens.VisualScripting.Pins;

public class OutputPinCollectionViewModel : PinCollectionViewModel
{
    private readonly NodeScriptViewModel _nodeScriptViewModel;
    private readonly INodeVmFactory _nodeVmFactory;

    public OutputPinCollectionViewModel(IPinCollection outputPinCollection, NodeScriptViewModel nodeScriptViewModel, INodeVmFactory nodeVmFactory, INodeEditorService nodeEditorService)
        : base(outputPinCollection, nodeScriptViewModel, nodeEditorService)
    {
        _nodeScriptViewModel = nodeScriptViewModel;
        _nodeVmFactory = nodeVmFactory;
        OutputPinCollection = outputPinCollection;
    }

    public IPinCollection OutputPinCollection { get; }

    protected override PinViewModel CreatePinViewModel(IPin pin)
    {
        PinViewModel vm = _nodeVmFactory.OutputPinViewModel(pin, _nodeScriptViewModel);
        vm.RemovePin = RemovePin;
        return vm;
    }
}