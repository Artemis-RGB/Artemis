using Artemis.Core;

namespace Artemis.UI.Screens.Device.Layout.LayoutProviders;

public interface ILayoutProviderViewModel
{
    string Name { get; }

    string Description { get; }

    bool IsMatch(ArtemisDevice device);

    ArtemisDevice Device { get; set; }
}