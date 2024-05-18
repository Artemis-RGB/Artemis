using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Artemis.Core.DeviceProviders;

namespace Artemis.Core.Services;

/// <summary>
///     A service that allows you manage an <see cref="ArtemisDevice" />
/// </summary>
public interface IDeviceService : IArtemisService
{
    /// <summary>
    ///     Gets a read-only collection containing all enabled devices
    /// </summary>
    IReadOnlyCollection<ArtemisDevice> EnabledDevices { get; }

    /// <summary>
    ///     Gets a read-only collection containing all registered devices
    /// </summary>
    IReadOnlyCollection<ArtemisDevice> Devices { get; }

    /// <summary>
    ///     Identifies the device by making it blink white 5 times
    /// </summary>
    /// <param name="device"></param>
    void IdentifyDevice(ArtemisDevice device);

    /// <summary>
    ///     Adds the given device provider and its devices.
    /// </summary>
    /// <param name="deviceProvider"></param>
    void AddDeviceProvider(DeviceProvider deviceProvider);

    /// <summary>
    ///     Removes the given device provider and its devices.
    /// </summary>
    /// <param name="deviceProvider"></param>
    void RemoveDeviceProvider(DeviceProvider deviceProvider);

    /// <summary>
    ///     Applies auto-arranging logic to the surface
    /// </summary>
    void AutoArrangeDevices();
    
    /// <summary>
    ///     Apples the best available to the provided <see cref="ArtemisDevice" />
    /// </summary>
    /// <param name="device"></param>
    void LoadDeviceLayout(ArtemisDevice device);

    /// <summary>
    ///     Enables the provided device
    /// </summary>
    /// <param name="device">The device to enable</param>
    void EnableDevice(ArtemisDevice device);

    /// <summary>
    ///     Disables the provided device
    /// </summary>
    /// <param name="device">The device to disable</param>
    void DisableDevice(ArtemisDevice device);

    /// <summary>
    ///     Saves the configuration of the provided device to persistent storage
    /// </summary>
    /// <param name="artemisDevice"></param>
    void SaveDevice(ArtemisDevice artemisDevice);

    /// <summary>
    ///     Saves the configuration of all current devices to persistent storage
    /// </summary>
    void SaveDevices();

    /// <summary>
    ///     Suspends all active device providers
    /// </summary>
    void SuspendDeviceProviders();

    /// <summary>
    ///     Resumes all previously active device providers
    /// </summary>
    void ResumeDeviceProviders();

    /// <summary>
    ///     Occurs when a single device was added.
    /// </summary>
    event EventHandler<DeviceEventArgs> DeviceAdded;

    /// <summary>
    ///     Occurs when a single device was removed.
    /// </summary>
    event EventHandler<DeviceEventArgs> DeviceRemoved;

    /// <summary>
    ///     Occurs when a single device was disabled
    /// </summary>
    event EventHandler<DeviceEventArgs> DeviceEnabled;

    /// <summary>
    ///     Occurs when a single device was disabled.
    /// </summary>
    event EventHandler<DeviceEventArgs> DeviceDisabled;

    /// <summary>
    ///     Occurs when a device provider was added.
    /// </summary>
    event EventHandler<DeviceProviderEventArgs> DeviceProviderAdded;

    /// <summary>
    ///     Occurs when a device provider was removed.
    /// </summary>
    event EventHandler<DeviceProviderEventArgs> DeviceProviderRemoved;
    
    /// <summary>
    ///     Occurs when the surface has had modifications to its LED collection
    /// </summary>
    event EventHandler LedsChanged;
}