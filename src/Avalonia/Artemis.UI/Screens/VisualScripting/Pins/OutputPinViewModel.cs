using Artemis.Core;
using Artemis.Core.Services;

namespace Artemis.UI.Screens.VisualScripting.Pins;

public class OutputPinViewModel : PinViewModel
{
    public OutputPinViewModel(IPin outputPin, INodeService nodeService) : base(outputPin, nodeService)
    {
    }
}