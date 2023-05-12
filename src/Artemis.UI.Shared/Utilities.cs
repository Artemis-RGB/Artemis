using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;
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
    private static readonly BehaviorSubject<bool> KeyBindingsEnabledSubject = new(false);

    static UI()
    {
        if (KeyboardDevice.Instance != null)
            KeyboardDevice.Instance.PropertyChanged += InstanceOnPropertyChanged;
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
    public static IObservable<bool> KeyBindingsEnabled { get; } = KeyBindingsEnabledSubject.AsObservable();

    private static void InstanceOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (KeyboardDevice.Instance == null || e.PropertyName != nameof(KeyboardDevice.FocusedElement))
            return;

        bool enabled = KeyboardDevice.Instance.FocusedElement is not TextBox;
        if (KeyBindingsEnabledSubject.Value != enabled)
            KeyBindingsEnabledSubject.OnNext(enabled);
    }
}