using Artemis.Core;
using Artemis.Core.Providers;
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
        InstalledEntry? layoutEntry = _workshopService.GetInstalledEntries().FirstOrDefault(e => e.EntryType == EntryType.Layout && MatchesDevice(e, device));
        if (layoutEntry == null)
            return null;

        string layoutPath = Path.Combine(Constants.WorkshopFolder, layoutEntry.EntryId.ToString(), "layout.xml");
        if (!File.Exists(layoutPath))
            return null;
        
        return new ArtemisLayout(layoutPath);
    }

    /// <inheritdoc />
    public void ApplyLayout(ArtemisDevice device, ArtemisLayout layout)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public bool IsMatch(ArtemisDevice device)
    {
        return device.LayoutSelection.Type == LayoutType;
    }

    private bool MatchesDevice(InstalledEntry entry, ArtemisDevice device)
    {
        return entry.TryGetMetadata("DeviceId", out HashSet<string>? deviceIds) && deviceIds.Contains(device.Identifier);
    }
}