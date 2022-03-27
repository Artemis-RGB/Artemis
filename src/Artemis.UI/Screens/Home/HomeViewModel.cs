using ReactiveUI;

namespace Artemis.UI.Screens.Home
{
    public class HomeViewModel : MainScreenViewModel
    {
        public HomeViewModel(IScreen hostScreen) : base(hostScreen, "home")
        {
            DisplayName = "Home";
        }
    }
}