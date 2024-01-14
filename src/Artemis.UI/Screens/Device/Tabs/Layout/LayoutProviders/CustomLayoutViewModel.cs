using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Providers;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Builders;

namespace Artemis.UI.Screens.Device.Layout.LayoutProviders;

public class CustomLayoutViewModel : ViewModelBase, ILayoutProviderViewModel
{
    private readonly CustomPathLayoutProvider _layoutProvider;

    public CustomLayoutViewModel(IWindowService windowService, INotificationService notificationService, IDeviceService deviceService, CustomPathLayoutProvider layoutProvider)
    {
        _layoutProvider = layoutProvider;
        _windowService = windowService;
        _notificationService = notificationService;
        _deviceService = deviceService;
    }

    /// <inheritdoc />
    public string Name => "Custom";

    /// <inheritdoc />
    public string Description => "Select a layout file from a folder on your computer";

    /// <inheritdoc />
    public ILayoutProvider Provider => _layoutProvider;

    public ArtemisDevice Device { get; set; } = null!;

    private readonly IWindowService _windowService;

    private readonly INotificationService _notificationService;
    private readonly IDeviceService _deviceService;

    public void ClearCustomLayout()
    {
        _layoutProvider.ConfigureDevice(Device, null);
        Save();
        
        _notificationService.CreateNotification()
            .WithMessage("Cleared imported layout.")
            .WithSeverity(NotificationSeverity.Informational);
    }

    public async Task BrowseCustomLayout()
    {
        string[]? files = await _windowService.CreateOpenFileDialog()
            .WithTitle("Select device layout file")
            .HavingFilter(f => f.WithName("Layout files").WithExtension("xml"))
            .ShowAsync();

        if (files?.Length > 0)
        {
            _layoutProvider.ConfigureDevice(Device, files[0]);
            Save();
            
            _notificationService.CreateNotification()
                .WithTitle("Imported layout")
                .WithMessage($"File loaded from {files[0]}")
                .WithSeverity(NotificationSeverity.Informational);
        }
    }

    /// <inheritdoc />
    public void Apply()
    {
        _layoutProvider.ConfigureDevice(Device, null);
        Save();
    }

    private void Save()
    {
        _deviceService.SaveDevice(Device);
        _deviceService.LoadDeviceLayout(Device);
    }
}