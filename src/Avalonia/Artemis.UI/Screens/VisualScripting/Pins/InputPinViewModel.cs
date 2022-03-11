using Artemis.Core;

namespace Artemis.UI.Screens.VisualScripting.Pins;

public class InputPinViewModel<T> : PinViewModel
{
    public InputPin<T> InputPin { get; }

    public InputPinViewModel(InputPin<T> inputPin) : base(inputPin)
    {
        InputPin = inputPin;
    }
}