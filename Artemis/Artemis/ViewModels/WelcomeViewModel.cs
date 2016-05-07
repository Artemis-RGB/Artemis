using System.Diagnostics;
using Caliburn.Micro;

namespace Artemis.ViewModels
{
    public sealed class WelcomeViewModel : Screen
    {
        public WelcomeViewModel()
        {
            DisplayName = "Welcome";
        }

        public void NavigateTo(string url)
        {
            Process.Start(new ProcessStartInfo(url));
        }
    }
}