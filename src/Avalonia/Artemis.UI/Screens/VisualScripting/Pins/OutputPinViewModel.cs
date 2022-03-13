using Artemis.Core;
using Artemis.Core.Services;

namespace Artemis.UI.Screens.VisualScripting.Pins;

public class OutputPinViewModel<T> : PinViewModel
{
    public OutputPin<T> OutputPin { get; }

    public OutputPinViewModel(OutputPin<T> outputPin, INodeService nodeService) : base(outputPin, nodeService)
    {
        OutputPin = outputPin;
    }
}