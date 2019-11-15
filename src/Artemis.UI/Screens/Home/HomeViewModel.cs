using System;
using System.Diagnostics;
using Stylet;

namespace Artemis.UI.Screens.Home
{
    public class HomeViewModel : Screen, IScreenViewModel
    {
        public string Title => "Home";

        public void OpenUrl(string url)
        {
            // Don't open anything but valid URIs
            if (Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
                Process.Start(url);
        }
    }
}