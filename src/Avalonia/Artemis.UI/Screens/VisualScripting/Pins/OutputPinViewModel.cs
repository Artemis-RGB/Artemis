using Artemis.Core;

namespace Artemis.UI.Screens.VisualScripting.Pins;

public class OutputPinViewModel<T> : PinViewModel
{
    public OutputPin<T> OutputPin { get; }

    public OutputPinViewModel(OutputPin<T> outputPin) : base(outputPin)
    {
        OutputPin = outputPin;
    }
}