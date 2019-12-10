using System;
using System.Diagnostics;
using MaterialDesignThemes.Wpf;

namespace Artemis.UI.Screens.Home
{
    public class HomeViewModel : MainScreenViewModel
    {
        public HomeViewModel()
        {
            DisplayName = "Home";
            DisplayIcon = PackIconKind.Home;
            DisplayOrder = 1;
        }

        public void OpenUrl(string url)
        {
            // Don't open anything but valid URIs
            if (Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
                Process.Start(url);
        }
    }
}