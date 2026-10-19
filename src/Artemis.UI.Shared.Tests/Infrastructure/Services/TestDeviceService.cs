using Artemis.Core.DeviceProviders;
using Artemis.Core.Services;

namespace Artemis.UI.Shared.Tests.Infrastructure.Services;

/// <summary>
///     Hands the profile editor a fixed list of enabled devices.
/// </summary>
public class TestDeviceService : IDeviceService
{
    /// <summary>
    ///     Gets the devices reported as enabled, add the devices of the render harness here
    /// </summary>
    public List<ArtemisDevice> EnabledDeviceList { get; } = [];

    public IReadOnlyCollection<ArtemisDevice> EnabledDevices => EnabledDeviceList;
    public IReadOnlyCollection<ArtemisDevice> Devices => EnabledDeviceList;
    public IReadOnlyCollection<DeviceProvider> SuspendedDeviceProviders { get; } = [];

    public void IdentifyDevice(ArtemisDevice device) => throw new NotSupportedException();
    public void AddDeviceProvider(DeviceProvider deviceProvider) => throw new NotSupportedException();
    public void RemoveDeviceProvider(DeviceProvider deviceProvider) => throw new NotSupportedException();
    public void AutoArrangeDevices(bool leftHanded) => throw new NotSupportedException();
    public void LoadDeviceLayout(ArtemisDevice device) => throw new NotSupportedException();
    public void EnableDevice(ArtemisDevice device) => throw new NotSupportedException();
    public void DisableDevice(ArtemisDevice device) => throw new NotSupportedException();
    public void SaveDevice(ArtemisDevice artemisDevice) => throw new NotSupportedException();
    public void SaveDevices() => throw new NotSupportedException();
    public void SuspendDeviceProviders() => throw new NotSupportedException();
    public void ResumeDeviceProviders() => throw new NotSupportedException();

#pragma warning disable CS0067 // Events are part of the interface but never raised here
    public event EventHandler<DeviceEventArgs>? DeviceAdded;
    public event EventHandler<DeviceEventArgs>? DeviceRemoved;
    public event EventHandler<DeviceEventArgs>? DeviceEnabled;
    public event EventHandler<DeviceEventArgs>? DeviceDisabled;
    public event EventHandler<DeviceProviderEventArgs>? DeviceProviderAdded;
    public event EventHandler<DeviceProviderEventArgs>? DeviceProviderRemoved;
    public event EventHandler? LedsChanged;
#pragma warning restore CS0067
}
