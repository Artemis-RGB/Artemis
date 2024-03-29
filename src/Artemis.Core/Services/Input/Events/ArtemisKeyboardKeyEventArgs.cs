﻿using System;

namespace Artemis.Core.Services;

/// <summary>
///     Contains data for keyboard input events
/// </summary>
public class ArtemisKeyboardKeyEventArgs : EventArgs
{
    internal ArtemisKeyboardKeyEventArgs(ArtemisDevice? device, ArtemisLed? led, KeyboardKey key, KeyboardModifierKey modifiers)
    {
        Device = device;
        Led = led;
        Key = key;
        Modifiers = modifiers;
    }

    /// <summary>
    ///     Gets the device that triggered the event
    /// </summary>
    public ArtemisDevice? Device { get; }

    /// <summary>
    ///     Gets the LED that corresponds to the pressed key
    /// </summary>
    public ArtemisLed? Led { get; }

    /// <summary>
    ///     Gets the key that triggered the event
    /// </summary>
    public KeyboardKey Key { get; }

    /// <summary>
    ///     Gets the modifiers that are pressed
    /// </summary>
    public KeyboardModifierKey Modifiers { get; }

    /// <summary>
    /// Creates a hotkey matching the event.
    /// </summary>
    /// <returns>The resulting hotkey.</returns>
    public Hotkey ToHotkey()
    {
        return new Hotkey {Key = Key, Modifiers = Modifiers};
    }
}