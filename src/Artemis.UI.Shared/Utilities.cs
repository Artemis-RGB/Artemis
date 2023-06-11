using System;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using IContainer = DryIoc.IContainer;

namespace Artemis.UI.Shared;

/// <summary>
/// Static UI helpers.
/// </summary>
public static class UI
{
    static UI()
    {
        KeyBindingsEnabled = InputElement.GotFocusEvent.Raised.Select(e => e.Item2.Source is not TextBox).StartWith(true);
    }

    /// <summary>
    ///     Gets the current IoC locator.
    /// </summary>
    public static IContainer Locator { get; set; } = null!;

    /// <summary>
    ///     Gets the clipboard.
    /// </summary>
    public static IClipboard Clipboard { get; set; } = null!;

    /// <summary>
    ///     Gets a boolean indicating whether hotkeys are to be disabled.
    /// </summary>
    public static IObservable<bool> KeyBindingsEnabled { get; }
}