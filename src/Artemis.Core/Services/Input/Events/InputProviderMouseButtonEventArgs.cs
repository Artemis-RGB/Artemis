using System;

namespace Artemis.Core.Services;

/// <summary>
///     Contains data for input provider mouse button events
/// </summary>
public class InputProviderMouseButtonEventArgs : EventArgs
{
    /// <summary>
    /// </summary>
    /// <param name="device">The device that triggered the event</param>
    /// <param name="button">The button that triggered the event</param>
    /// <param name="isDown">Whether the button is pressed down</param>
    public InputProviderMouseButtonEventArgs(ArtemisDevice? device, MouseButton button, bool isDown)
    {
        Device = device;
        Button = button;
        IsDown = isDown;
    }

    /// <summary>
    ///     Gets the device that triggered the event
    /// </summary>
    public ArtemisDevice? Device { get; }

    /// <summary>
    ///     Gets the button that triggered the event
    /// </summary>
    public MouseButton Button { get; }

    /// <summary>
    ///     Gets whether the button is pressed down
    /// </summary>
    public bool IsDown { get; }
}