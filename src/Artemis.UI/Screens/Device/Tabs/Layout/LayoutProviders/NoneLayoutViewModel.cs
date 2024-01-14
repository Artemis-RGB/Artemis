using Artemis.Core;
using Artemis.Core.Providers;
using Artemis.Core.Services;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.Device.Layout.LayoutProviders;

public class NoneLayoutViewModel : ViewModelBase, ILayoutProviderViewModel
{
    private readonly NoneLayoutProvider _layoutProvider;
    private readonly IDeviceService _deviceService;

    public NoneLayoutViewModel(NoneLayoutProvider layoutProvider, IDeviceService deviceService)
    {
        _layoutProvider = layoutProvider;
        _deviceService = deviceService;
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
        Save();
    }
    
    private void Save()
    {
        _deviceService.SaveDevice(Device);
        _deviceService.LoadDeviceLayout(Device);
    }
}