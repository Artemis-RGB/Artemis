using System.Diagnostics;
using Artemis.ViewModels.Abstract;

namespace Artemis.ViewModels
{
    public sealed class WelcomeViewModel : BaseViewModel
    {
        public WelcomeViewModel()
        {
            DisplayName = "Welcome";
        }

        public void NavigateTo(string url)
        {
            System.Diagnostics.Process.Start(new ProcessStartInfo(url));
        }
    }
}