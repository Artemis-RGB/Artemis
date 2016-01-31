using System.Diagnostics;
using MahApps.Metro.Controls;

namespace Artemis.ViewModels.Flyouts
{
    public class FlyoutSettingsViewModel : FlyoutBaseViewModel
    {
        public FlyoutSettingsViewModel()
        {
            Header = "settings";
            Position = Position.Right;
        }

        public void NavigateTo(string url)
        {
            Process.Start(new ProcessStartInfo(url));
        }
    }
}