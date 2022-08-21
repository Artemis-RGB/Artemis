namespace Artemis.Core.Services;

/// <summary>
///     Contains data for mouse input events
/// </summary>
public class ArtemisMouseButtonUpDownEventArgs : ArtemisMouseButtonEventArgs
{
    internal ArtemisMouseButtonUpDownEventArgs(ArtemisDevice? device, ArtemisLed? led, MouseButton button, bool isDown) : base(device, led, button)
    {
        IsDown = isDown;
    }

    /// <summary>
    ///     Whether the button is being pressed down, if <see langword="false" /> the button is being released
    /// </summary>
    public bool IsDown { get; set; }
}