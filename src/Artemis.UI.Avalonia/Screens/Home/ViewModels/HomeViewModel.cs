using ReactiveUI;

namespace Artemis.UI.Avalonia.Screens.Home.ViewModels
{
    public class HomeViewModel : MainScreenViewModel
    {
        public HomeViewModel(IScreen hostScreens) : base(hostScreens, "home")
        {
            DisplayName = "Home";
        }

        public void OpenUrl(string url)
        {
            Core.Utilities.OpenUrl(url);
        }
    }
}