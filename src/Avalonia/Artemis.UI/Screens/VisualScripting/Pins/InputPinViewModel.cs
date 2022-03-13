using Artemis.Core;
using Artemis.Core.Services;

namespace Artemis.UI.Screens.VisualScripting.Pins;

public class InputPinViewModel<T> : PinViewModel
{
    public InputPin<T> InputPin { get; }

    public InputPinViewModel(InputPin<T> inputPin, INodeService nodeService) : base(inputPin, nodeService)
    {
        InputPin = inputPin;
    }
}