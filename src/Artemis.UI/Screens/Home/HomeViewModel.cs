using System;
using System.Diagnostics;

namespace Artemis.UI.Screens.Home
{
    public class HomeViewModel : MainScreenViewModel
    {
        public HomeViewModel()
        {
            DisplayName = "Home";
        }

        public void OpenUrl(string url)
        {
            // Don't open anything but valid URIs
            if (Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
                Process.Start(url);
        }
    }
}