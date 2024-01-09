namespace Artemis.Core.Providers;

public class NoneLayoutProvider : ILayoutProvider
{
    public static string LayoutType = "None";

    /// <inheritdoc />
    public ArtemisLayout? GetDeviceLayout(ArtemisDevice device)
    {
        return null;
    }

    /// <inheritdoc />
    public void ApplyLayout(ArtemisDevice device, ArtemisLayout layout)
    {
        device.ApplyLayout(null, false, false);
    }
    
    /// <inheritdoc />
    public bool IsMatch(ArtemisDevice device)
    {
        return device.LayoutSelection.Type == LayoutType;
    }
}