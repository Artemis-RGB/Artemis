using Artemis.Core;

namespace Artemis.UI.Screens.VisualScripting.Pins;

public class InputPinCollectionViewModel<T> : PinCollectionViewModel
{
    public InputPinCollection<T> InputPinCollection { get; }

    public InputPinCollectionViewModel(InputPinCollection<T> inputPinCollection) : base(inputPinCollection)
    {
        InputPinCollection = inputPinCollection;
    }
}