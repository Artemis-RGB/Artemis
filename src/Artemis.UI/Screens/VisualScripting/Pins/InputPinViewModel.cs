using Artemis.Core;
using Artemis.Core.Services;

namespace Artemis.UI.Screens.VisualScripting.Pins;

public class InputPinViewModel : PinViewModel
{
    public InputPinViewModel(IPin inputPin, INodeService nodeService) : base(inputPin, nodeService)
    {
    }
}