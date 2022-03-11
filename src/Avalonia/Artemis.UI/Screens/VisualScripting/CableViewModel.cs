using Artemis.UI.Screens.VisualScripting.Pins;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.VisualScripting;

public class CableViewModel : ActivatableViewModelBase
{
    private PinViewModel _from;
    private PinViewModel _to;

    public CableViewModel(PinViewModel from, PinViewModel to)
    {
        _from = from;
        _to = to;
    }

    public PinViewModel From
    {
        get => _from;
        set => RaiseAndSetIfChanged(ref _from, value);
    }

    public PinViewModel To
    {
        get => _to;
        set => RaiseAndSetIfChanged(ref _to, value);
    }
}