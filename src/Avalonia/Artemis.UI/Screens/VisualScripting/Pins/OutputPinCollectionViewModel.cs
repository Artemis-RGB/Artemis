using Artemis.Core;
using Artemis.UI.Ninject.Factories;

namespace Artemis.UI.Screens.VisualScripting.Pins;

public class OutputPinCollectionViewModel<T> : PinCollectionViewModel
{
    public OutputPinCollection<T> OutputPinCollection { get; }

    public OutputPinCollectionViewModel(OutputPinCollection<T> outputPinCollection, INodePinVmFactory nodePinVmFactory) : base(outputPinCollection, nodePinVmFactory)
    {
        OutputPinCollection = outputPinCollection;
    }
}