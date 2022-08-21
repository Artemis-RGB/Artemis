using System;

namespace Artemis.Core.Services;

/// <summary>
///     Contains data for keyboard input events
/// </summary>
public class ArtemisKeyboardToggleStatusArgs : EventArgs
{
    internal ArtemisKeyboardToggleStatusArgs(KeyboardToggleStatus oldStatus, KeyboardToggleStatus newStatus)
    {
        OldStatus = oldStatus;
        NewStatus = newStatus;
    }

    /// <summary>
    ///     Gets the keyboard status before the change
    /// </summary>
    public KeyboardToggleStatus OldStatus { get; }

    /// <summary>
    ///     Gets the keyboard status after the change
    /// </summary>
    public KeyboardToggleStatus NewStatus { get; }
}