using Artemis.Core;
using Artemis.Core.Providers;
using Artemis.WebClient.Workshop.Models;
using Artemis.WebClient.Workshop.Services;

namespace Artemis.WebClient.Workshop.Providers;

public class WorkshopLayoutProvider : ILayoutProvider
{
    public static string LayoutType = "Workshop";
    private readonly IWorkshopService _workshopService;

    public WorkshopLayoutProvider(IWorkshopService workshopService)
    {
        _workshopService = workshopService;
    }

    /// <inheritdoc />
    public ArtemisLayout? GetDeviceLayout(ArtemisDevice device)
    {
        InstalledEntry? layoutEntry = _workshopService.GetInstalledEntries().FirstOrDefault(e => e.EntryId.ToString() == device.LayoutSelection.Parameter);
        if (layoutEntry == null)
            return null;

        string layoutPath = Path.Combine(layoutEntry.GetReleaseDirectory().FullName, "layout.xml");
        if (!File.Exists(layoutPath))
            return null;

        return new ArtemisLayout(layoutPath);
    }

    /// <inheritdoc />
    public void ApplyLayout(ArtemisDevice device, ArtemisLayout layout)
    {
        device.ApplyLayout(layout, device.DeviceProvider.CreateMissingLedsSupported, device.DeviceProvider.RemoveExcessiveLedsSupported);
    }

    /// <inheritdoc />
    public bool IsMatch(ArtemisDevice device)
    {
        return device.LayoutSelection.Type == LayoutType;
    }

    /// <summary>
    /// Configures the provided device to use this layout provider.
    /// </summary>
    /// <param name="device">The device to apply the provider to.</param>
    /// <param name="entry">The workshop entry to use as a layout.</param>
    public void ConfigureDevice(ArtemisDevice device, InstalledEntry? entry)
    {
        if (entry != null && entry.EntryType != EntryType.Layout)
            throw new InvalidOperationException($"Cannot use a workshop entry of type {entry.EntryType} as a layout");

        device.LayoutSelection.Type = LayoutType;
        device.LayoutSelection.Parameter = entry?.EntryId.ToString();
    }
}