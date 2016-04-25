using System.Diagnostics;
using Caliburn.Micro;

namespace Artemis.ViewModels
{
    public class WelcomeViewModel : Screen
    {
        public void NavigateTo(string url)
        {
            Process.Start(new ProcessStartInfo(url));
        }
    }
}