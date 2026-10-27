using System;

namespace Artemis.UI.Services.Tray;

/// <summary>
///     Provides the light or dark mode of the system tray.
/// </summary>
public interface ITrayThemeProvider
{
    /// <summary>
    ///     Gets a value indicating whether the system tray has a dark background.
    /// </summary>
    bool IsTrayDark { get; }

    /// <summary>
    ///     Occurs when the background of the system tray may have changed, possibly on a background thread.
    /// </summary>
    event EventHandler? TrayThemeChanged;
}
