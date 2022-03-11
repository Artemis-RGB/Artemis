using Artemis.Core;

namespace Artemis.UI.Screens.VisualScripting.Pins;

public class OutputPinCollectionViewModel<T> : PinCollectionViewModel
{
    public OutputPinCollection<T> OutputPinCollection { get; }

    public OutputPinCollectionViewModel(OutputPinCollection<T> outputPinCollection) : base(outputPinCollection)
    {
        OutputPinCollection = outputPinCollection;
    }
}