using Artemis.Core;
using Artemis.UI.Shared.Routing;

namespace Artemis.UI.Screens.Settings;

public class AboutTabViewModel : RoutableScreen
{
    public AboutTabViewModel()
    {
        DisplayName = "About";
        Version = $"Version {Constants.CurrentVersion}";
    }

    public string Version { get; }
}