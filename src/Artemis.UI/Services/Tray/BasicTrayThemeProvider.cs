using System;
using Avalonia;
using Avalonia.Platform;

namespace Artemis.UI.Services.Tray;

/// <summary>
///     Provides the light or dark mode of the system tray based on the light or dark mode of the operating system.
/// </summary>
public class BasicTrayThemeProvider : ITrayThemeProvider
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BasicTrayThemeProvider" /> class.
    /// </summary>
    public BasicTrayThemeProvider()
    {
        if (Application.Current?.PlatformSettings is { } platformSettings)
            platformSettings.ColorValuesChanged += (_, _) => TrayThemeChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public bool IsTrayDark => Application.Current?.PlatformSettings?.GetColorValues().ThemeVariant != PlatformThemeVariant.Light;

    /// <inheritdoc />
    public event EventHandler? TrayThemeChanged;
}
