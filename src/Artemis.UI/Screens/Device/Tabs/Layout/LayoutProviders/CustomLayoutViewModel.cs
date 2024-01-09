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
    private readonly IDeviceService _deviceService;

    public CustomLayoutViewModel(IWindowService windowService, INotificationService notificationService, CustomPathLayoutProvider layoutProvider, IDeviceService deviceService)
    {
        _layoutProvider = layoutProvider;
        _deviceService = deviceService;
        _windowService = windowService;
        _notificationService = notificationService;
    }

    /// <inheritdoc />
    public string Name => "Custom";

    /// <inheritdoc />
    public string Description => "Select a layout file from a folder on your computer";

    /// <inheritdoc />
    public bool IsMatch(ArtemisDevice device)
    {
        return _layoutProvider.IsMatch(device);
    }

    public ArtemisDevice Device { get; set; } = null!;
    
    private readonly IWindowService _windowService;
    private readonly INotificationService _notificationService;

    public void ClearCustomLayout()
    {
        Device.LayoutSelection.Type = CustomPathLayoutProvider.LayoutType;
        Device.LayoutSelection.Parameter = null;
        _notificationService.CreateNotification()
            .WithMessage("Cleared imported layout.")
            .WithSeverity(NotificationSeverity.Informational);
        
        _deviceService.SaveDevice(Device);
    }

    public async Task BrowseCustomLayout()
    {
        string[]? files = await _windowService.CreateOpenFileDialog()
            .WithTitle("Select device layout file")
            .HavingFilter(f => f.WithName("Layout files").WithExtension("xml"))
            .ShowAsync();

        if (files?.Length > 0)
        {
            Device.LayoutSelection.Type = CustomPathLayoutProvider.LayoutType;
            Device.LayoutSelection.Parameter = files[0];
            _notificationService.CreateNotification()
                .WithTitle("Imported layout")
                .WithMessage($"File loaded from {files[0]}")
                .WithSeverity(NotificationSeverity.Informational);
        }
        
        _deviceService.SaveDevice(Device);
    }
}