using ReactiveUI;

namespace Artemis.UI.Avalonia.Screens.Home.ViewModels
{
    public class HomeViewModel : MainScreenViewModel
    {
        public HomeViewModel(IScreen hostScreen) : base(hostScreen, "home")
        {
            DisplayName = "Home";
        }
    }
}