using System.Windows.Navigation;
using Stylet;

namespace Artemis.UI.Screens.StartupWizard.Steps
{
    public class FinishStepViewModel : Screen
    {
        public void OpenHyperlink(object sender, RequestNavigateEventArgs e)
        {
            Core.Utilities.OpenUrl(e.Uri.AbsoluteUri);
        }

        public void Finish()
        {
            ((StartupWizardViewModel) Parent).SkipOrFinishWizard();
        }
    }
}