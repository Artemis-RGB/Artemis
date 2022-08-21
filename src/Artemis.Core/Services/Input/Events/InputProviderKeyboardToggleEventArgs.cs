using System;

namespace Artemis.Core.Services;

/// <summary>
///     Contains data for input provider keyboard status toggle events
/// </summary>
public class InputProviderKeyboardToggleEventArgs : EventArgs
{
    /// <summary>
    ///     Creates a new instance of the <see cref="InputProviderKeyboardToggleEventArgs " /> class
    /// </summary>
    /// <param name="keyboardToggleStatus">The toggle status of the keyboard</param>
    public InputProviderKeyboardToggleEventArgs(KeyboardToggleStatus keyboardToggleStatus)
    {
        KeyboardToggleStatus = keyboardToggleStatus;
    }

    /// <summary>
    ///     Gets the toggle status of the keyboard
    /// </summary>
    public KeyboardToggleStatus KeyboardToggleStatus { get; }
}