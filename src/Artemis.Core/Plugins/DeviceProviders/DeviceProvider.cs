using System;
using System.IO;
using System.Linq;
using RGB.NET.Core;

namespace Artemis.Core.DeviceProviders;

/// <inheritdoc />
/// <summary>
///     Allows you to implement and register your own device provider
/// </summary>
public abstract class DeviceProvider : PluginFeature
{
    /// <summary>
    ///     The RGB.NET device provider backing this Artemis device provider
    /// </summary>
    public abstract IRGBDeviceProvider RgbDeviceProvider { get; }
    
    /// <summary>
    ///     A boolean indicating whether this device provider detects the physical layout of connected keyboards.
    ///     <para>
    ///         Note: <see cref="GetLogicalLayout" /> is only called when this or <see cref="CanDetectLogicalLayout" />
    ///         is <see langword="true" />.
    ///     </para>
    /// </summary>
    public bool CanDetectPhysicalLayout { get; protected set; }

    /// <summary>
    ///     A boolean indicating whether this device provider detects the logical layout of connected keyboards
    ///     <para>
    ///         Note: <see cref="GetLogicalLayout" /> is only called when this or <see cref="CanDetectPhysicalLayout" />
    ///         is <see langword="true" />.
    ///     </para>
    /// </summary>
    public bool CanDetectLogicalLayout { get; protected set; }

    /// <summary>
    ///     Gets or sets a boolean indicating whether adding missing LEDs defined in a layout but missing on the device is
    ///     supported
    ///     <para>Note: Defaults to <see langword="true" />.</para>
    /// </summary>
    public bool CreateMissingLedsSupported { get; protected set; } = true;

    /// <summary>
    ///     Gets or sets a boolean indicating whether removing excess LEDs present in the device but missing in the layout is
    ///     supported
    ///     <para>Note: Defaults to <see langword="true" />.</para>
    /// </summary>
    public bool RemoveExcessiveLedsSupported { get; protected set; } = true;

    /// <summary>
    ///     Loads a layout for the specified device and wraps it in an <see cref="ArtemisLayout" />
    /// </summary>
    /// <param name="device">The device to load the layout for</param>
    /// <returns>The resulting Artemis layout</returns>
    public virtual ArtemisLayout LoadLayout(ArtemisDevice device)
    {
        string layoutDir = Path.Combine(Plugin.Directory.FullName, "Layouts");
        string filePath = Path.Combine(
            layoutDir,
            device.RgbDevice.DeviceInfo.Manufacturer,
            device.DeviceType.ToString(),
            GetDeviceLayoutName(device)
        );
        return new ArtemisLayout(filePath);
    }

    /// <summary>
    ///     Loads a layout from the user layout folder for the specified device and wraps it in an <see cref="ArtemisLayout" />
    /// </summary>
    /// <param name="device">The device to load the layout for</param>
    /// <returns>The resulting Artemis layout</returns>
    public virtual ArtemisLayout LoadUserLayout(ArtemisDevice device)
    {
        string layoutDir = Constants.LayoutsFolder;
        string filePath = Path.Combine(
            layoutDir,
            device.RgbDevice.DeviceInfo.Manufacturer,
            device.DeviceType.ToString(),
            GetDeviceLayoutName(device)
        );
        return new ArtemisLayout(filePath);
    }

    /// <summary>
    ///     Called when a specific RGB device's logical and physical layout must be detected
    ///     <para>
    ///         Note: Only called when <see cref="CanDetectLogicalLayout" /> is <see langword="true" />.
    ///     </para>
    /// </summary>
    /// <param name="keyboard">The device to detect the layout for, always a keyboard</param>
    public virtual string GetLogicalLayout(IKeyboard keyboard)
    {
        throw new NotImplementedException("Device provider does not support detecting logical layouts (don't call base.GetLogicalLayout())");
    }

    /// <summary>
    ///     Called when determining which file name to use when loading the layout of the specified
    ///     <paramref name="device"></paramref>.
    /// </summary>
    /// <param name="device">The device to determine the layout file name for.</param>
    /// <returns>A file name, including an extension</returns>
    public virtual string GetDeviceLayoutName(ArtemisDevice device)
    {
        // Take out invalid file name chars, may not be perfect but neither are you
        string fileName = Path.GetInvalidFileNameChars().Aggregate(device.RgbDevice.DeviceInfo.Model, (current, c) => current.Replace(c, '-'));
        if (device.RgbDevice.DeviceInfo.DeviceType == RGBDeviceType.Keyboard)
            fileName = $"{fileName}-{device.PhysicalLayout.ToString().ToUpper()}";

        return fileName + ".xml";
    }
}