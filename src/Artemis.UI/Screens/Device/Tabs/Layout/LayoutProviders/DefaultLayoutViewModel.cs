using Artemis.Core;
using Artemis.Core.Providers;
using Artemis.Core.Services;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.Device.Layout.LayoutProviders;

public class DefaultLayoutViewModel : ViewModelBase, ILayoutProviderViewModel
{
    private readonly DefaultLayoutProvider _layoutProvider;
    private readonly IDeviceService _deviceService;

    public DefaultLayoutViewModel(DefaultLayoutProvider layoutProvider, IDeviceService deviceService)
    {
        _layoutProvider = layoutProvider;
        _deviceService = deviceService;
    }

    /// <inheritdoc />
    public ILayoutProvider Provider => _layoutProvider;

    public ArtemisDevice Device { get; set; } = null!;

    /// <inheritdoc />
    public string Name => "Default";

    /// <inheritdoc />
    public string Description => "Attempts to load a layout from the default paths";

    /// <inheritdoc />
    public void Apply()
    {
        _layoutProvider.ConfigureDevice(Device);
        Save();
    }
    
    private void Save()
    {
        _deviceService.SaveDevice(Device);
        _deviceService.LoadDeviceLayout(Device);
    }
}