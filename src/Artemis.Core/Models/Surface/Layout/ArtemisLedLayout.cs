using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RGB.NET.Layout;

namespace Artemis.Core;

/// <summary>
///     Represents a LED layout decorated with extra Artemis-specific data
/// </summary>
public class ArtemisLedLayout
{
    internal ArtemisLedLayout(ArtemisLayout deviceLayout, ILedLayout led)
    {
        DeviceLayout = deviceLayout;
        RgbLayout = led;
        LayoutCustomLedData = (LayoutCustomLedData?) led.CustomData ?? new LayoutCustomLedData();

        // Default to the first logical layout for images
        LayoutCustomLedDataLogicalLayout? defaultLogicalLayout = LayoutCustomLedData.LogicalLayouts?.FirstOrDefault();
        if (defaultLogicalLayout != null)
            ApplyLogicalLayout(defaultLogicalLayout);
    }

    /// <summary>
    ///     Gets the device layout of this LED layout
    /// </summary>
    public ArtemisLayout DeviceLayout { get; }

    /// <summary>
    ///     Gets the RGB.NET LED Layout of this LED layout
    /// </summary>
    public ILedLayout RgbLayout { get; }

    /// <summary>
    ///     Gets the name of the logical layout this LED belongs to
    /// </summary>
    public string? LogicalName { get; private set; }

    /// <summary>
    ///     Gets the image of the LED
    /// </summary>
    public Uri? Image { get; private set; }

    /// <summary>
    ///     Gets the custom layout data embedded in the RGB.NET layout
    /// </summary>
    public LayoutCustomLedData LayoutCustomLedData { get; }

    /// <summary>
    ///   Gets the logical layout names available for this LED
    /// </summary>
    public IEnumerable<string> GetLogicalLayoutNames()
    {
        return LayoutCustomLedData.LogicalLayouts?.Where(l => l.Name != null).Select(l => l.Name!) ?? [];
    }

    internal void ApplyCustomLedData(ArtemisDevice artemisDevice)
    {
        if (LayoutCustomLedData.LogicalLayouts == null || !LayoutCustomLedData.LogicalLayouts.Any())
            return;

        // Prefer a matching layout or else a default layout (that has no name)
        LayoutCustomLedDataLogicalLayout logicalLayout = LayoutCustomLedData.LogicalLayouts
            .OrderByDescending(l => l.Name == artemisDevice.LogicalLayout)
            .ThenBy(l => l.Name == null)
            .First();

        ApplyLogicalLayout(logicalLayout);
    }

    private void ApplyLogicalLayout(LayoutCustomLedDataLogicalLayout logicalLayout)
    {
        string? layoutDirectory = Path.GetDirectoryName(DeviceLayout.FilePath);

        LogicalName = logicalLayout.Name;
        if (layoutDirectory != null && logicalLayout.Image != null)
            Image = new Uri(Path.Combine(layoutDirectory, logicalLayout.Image!), UriKind.Absolute);
        else
            Image = null;
    }
}