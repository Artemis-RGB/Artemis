using System;
using Artemis.Core;
using Avalonia.Input;

namespace Artemis.UI.Shared.Events;

/// <summary>
///     Provides data on LED click events raised by the device visualizer
/// </summary>
public class LedClickedEventArgs : EventArgs
{
    internal LedClickedEventArgs(ArtemisDevice device, ArtemisLed led, PointerReleasedEventArgs pointerReleasedEventArgs)
    {
        Device = device;
        Led = led;
        PointerReleasedEventArgs = pointerReleasedEventArgs;
    }

    /// <summary>
    ///     Gets the device that was clicked.
    /// </summary>
    public ArtemisDevice Device { get; set; }

    /// <summary>
    ///     Gets the LED that was clicked.
    /// </summary>
    public ArtemisLed Led { get; set; }

    /// <summary>
    ///     Gets the original pointer released event arguments.
    /// </summary>
    public PointerReleasedEventArgs PointerReleasedEventArgs { get; }
}