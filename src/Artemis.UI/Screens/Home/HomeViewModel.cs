using Artemis.UI.Ninject.Factories;

namespace Artemis.UI.Screens.Home
{
    public class HomeViewModel : MainScreenViewModel
    {
        public HomeViewModel(IHeaderVmFactory headerVmFactory)
        {
            DisplayName = "Home";
            HeaderViewModel = headerVmFactory.SimpleHeaderViewModel(DisplayName);
        }

        public void OpenUrl(string url)
        {
            Core.Utilities.OpenUrl(url);
        }
    }
}