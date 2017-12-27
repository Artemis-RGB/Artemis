using System;
using System.Diagnostics;
using Artemis.UI.ViewModels.Interfaces;
using Stylet;

namespace Artemis.UI.ViewModels
{
    public class HomeViewModel : Screen, IMainViewModel
    {
        public string Title => "Home";

        public void OpenUrl(string url)
        {
            // Don't open anything but valid URIs
            if (Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
                Process.Start(url);
        }
    }

    public interface IMainViewModel : IArtemisViewModel
    {
    }
}