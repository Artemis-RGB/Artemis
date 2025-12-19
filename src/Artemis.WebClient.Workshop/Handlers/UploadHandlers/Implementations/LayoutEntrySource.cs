using Artemis.Core;

namespace Artemis.WebClient.Workshop.Handlers.UploadHandlers;

public class LayoutEntrySource : IEntrySource
{
    public LayoutEntrySource(ArtemisLayout layout)
    {
        Layout = layout;
    }

    public ArtemisLayout Layout { get; set; }
    public List<LayoutInfo> LayoutInfo { get; set; } = [];
    public Core.KeyboardLayoutType PhysicalLayout { get; set; }

    private List<LayoutCustomLedDataLogicalLayout> GetLogicalLayouts()
    {
        return Layout.Leds
            .Where(l => l.LayoutCustomLedData.LogicalLayouts != null)
            .SelectMany(l => l.LayoutCustomLedData.LogicalLayouts!)
            .Where(l => !string.IsNullOrWhiteSpace(l.Name))
            .DistinctBy(l => l.Name)
            .ToList();
    }
}

public class LayoutInfo
{
    public string Vendor { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public Guid DeviceProviderId { get; set; }
}