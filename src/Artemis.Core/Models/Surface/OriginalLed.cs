using RGB.NET.Core;

namespace Artemis.Core;

internal class OriginalLed
{
    public OriginalLed(Led source)
    {
        Id = source.Id;
        Location = source.Location;
        Size = source.Size;
        CustomData = source.CustomData;
    }

    public LedId Id { get; set; }
    public Point Location { get; set; }
    public Size Size { get; set; }
    public object? CustomData { get; set; }
}