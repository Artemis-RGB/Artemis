using Artemis.Core;
using Artemis.UI.Ninject.Factories;

namespace Artemis.UI.Screens.VisualScripting.Pins;

public class InputPinCollectionViewModel<T> : PinCollectionViewModel
{
    public InputPinCollection<T> InputPinCollection { get; }

    public InputPinCollectionViewModel(InputPinCollection<T> inputPinCollection, INodePinVmFactory nodePinVmFactory) : base(inputPinCollection, nodePinVmFactory)
    {
        InputPinCollection = inputPinCollection;
    }
}