using Artemis.Core;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.VisualScripting.Pins;

public abstract class PinCollectionViewModel : ActivatableViewModelBase
{
    public PinCollection PinCollection { get; }

    protected PinCollectionViewModel(PinCollection pinCollection)
    {
        PinCollection = pinCollection;
    }
}