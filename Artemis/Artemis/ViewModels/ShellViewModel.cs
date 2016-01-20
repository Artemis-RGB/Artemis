using Caliburn.Micro;

namespace Artemis.ViewModels
{
    internal sealed class SettingsViewModel : Conductor<IScreen>.Collection.OneActive
    {
        public SettingsViewModel()
        {
            DisplayName = "Artemis - Settings";
        }
    }
}