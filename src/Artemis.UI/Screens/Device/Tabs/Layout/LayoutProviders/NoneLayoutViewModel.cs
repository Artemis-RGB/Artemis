using Artemis.Core;
using Artemis.Core.Providers;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.Device.Layout.LayoutProviders;

public class NoneLayoutViewModel : ViewModelBase, ILayoutProviderViewModel
{
    private readonly NoneLayoutProvider _layoutProvider;

    public NoneLayoutViewModel(NoneLayoutProvider layoutProvider)
    {
        _layoutProvider = layoutProvider;
    }

    /// <inheritdoc />
    public ILayoutProvider Provider => _layoutProvider;

    public ArtemisDevice Device { get; set; } = null!;

    /// <inheritdoc />
    public string Name => "None";

    /// <inheritdoc />
    public string Description => "Do not load any layout";

    /// <inheritdoc />
    public void Apply()
    {
        _layoutProvider.ConfigureDevice(Device);
    }
}