using Artemis.Core;
using Artemis.Core.Providers;

namespace Artemis.UI.Screens.Device.Layout.LayoutProviders;

public interface ILayoutProviderViewModel
{
    string Name { get; }

    string Description { get; }

    ILayoutProvider Provider { get; }

    ArtemisDevice Device { get; set; }

    void Apply();
}