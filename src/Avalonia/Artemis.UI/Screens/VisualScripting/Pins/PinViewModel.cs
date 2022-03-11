using Artemis.Core;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.VisualScripting.Pins;

public abstract class PinViewModel : ActivatableViewModelBase
{
    protected PinViewModel(IPin pin)
    {
        Pin = pin;
    }

    public IPin Pin { get; }
}