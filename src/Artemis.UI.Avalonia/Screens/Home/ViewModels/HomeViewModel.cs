namespace Artemis.UI.Avalonia.Screens.Home.ViewModels
{
    public class HomeViewModel : MainScreenViewModel
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