using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using Avalonia.Media;

namespace Artemis.UI.Screens.VisualScripting.Pins;

public abstract class PinViewModel : ActivatableViewModelBase
{
    protected PinViewModel(IPin pin, INodeService nodeService)
    {
        Pin = pin;

        TypeColorRegistration registration = nodeService.GetTypeColorRegistration(Pin.Type);
        PinColor = new Color(registration.Color.Alpha, registration.Color.Red, registration.Color.Green, registration.Color.Blue);
        DarkenedPinColor = new Color(registration.DarkenedColor.Alpha, registration.DarkenedColor.Red, registration.DarkenedColor.Green, registration.DarkenedColor.Blue);
    }

    public IPin Pin { get; }
    public Color PinColor { get; }
    public Color DarkenedPinColor { get; }
}