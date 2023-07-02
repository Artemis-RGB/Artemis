using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
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
    
    public static EventLoopScheduler BackgroundScheduler = new EventLoopScheduler(ts => new Thread(ts));

    internal static void ClearCache()
    {
        DeviceVisualizer.BitmapCache.Clear();
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