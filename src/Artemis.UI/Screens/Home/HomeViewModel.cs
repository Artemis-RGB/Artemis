using Stylet;

namespace Artemis.UI.Screens.Home
{
    public class HomeViewModel : Screen, IMainScreenViewModel
    {
        public HomeViewModel()
        {
            DisplayName = "Home";
        }

        public void OpenUrl(string url)
        {
            Core.Utilities.OpenUrl(url);
        }
    }
}