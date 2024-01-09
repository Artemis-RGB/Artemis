using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.WebClient.Workshop.Providers;

namespace Artemis.UI.Screens.Device.Layout.LayoutProviders;

public class WorkshopLayoutViewModel : ViewModelBase, ILayoutProviderViewModel
{
    private readonly WorkshopLayoutProvider _layoutProvider;

    public WorkshopLayoutViewModel(WorkshopLayoutProvider layoutProvider)
    {
        _layoutProvider = layoutProvider;
    }

    public ArtemisDevice Device { get; set; } = null!;

    /// <inheritdoc />
    public string Name => "Workshop";

    /// <inheritdoc />
    public string Description => "Load a layout from the workshop";

    /// <inheritdoc />
    public bool IsMatch(ArtemisDevice device)
    {
        return _layoutProvider.IsMatch(device);
    }
}