using Artemis.UI.Shared;
using Material.Icons;

namespace Artemis.UI.Screens.Sidebar;

public class ProfileIconViewModel : ViewModelBase
{
    private MaterialIconKind _icon;

    public ProfileIconViewModel(MaterialIconKind icon)
    {
        Icon = icon;
    }

    public MaterialIconKind Icon
    {
        get => _icon;
        set
        {
            RaiseAndSetIfChanged(ref _icon, value);
            DisplayName = _icon.ToString();
        }
    }
}