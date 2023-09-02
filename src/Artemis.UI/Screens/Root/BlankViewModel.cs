using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;

namespace Artemis.UI.Screens.Root;

public class BlankViewModel : RoutableScreen, IMainScreenViewModel
{
    /// <inheritdoc />
    public ViewModelBase? TitleBarViewModel => null;
}