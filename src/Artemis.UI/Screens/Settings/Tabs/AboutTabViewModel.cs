using Artemis.Core;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.Settings;

public class AboutTabViewModel : ActivatableViewModelBase
{
    public AboutTabViewModel()
    {
        DisplayName = "About";
        Version = $"Version {Constants.CurrentVersion}";
    }

    public string Version { get; }
}