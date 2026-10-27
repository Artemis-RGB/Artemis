using System;
using Artemis.UI.Services.Tray;
using Microsoft.Win32;
using Serilog;

namespace Artemis.UI.Windows.Providers;

/// <summary>
///     Provides the light or dark mode of the Windows taskbar, which can differ from the app mode Avalonia reports.
/// </summary>
public class WindowsTrayThemeProvider : ITrayThemeProvider
{
    private const string PersonalizeKey = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

    /// <summary>
    ///     Initializes a new instance of the <see cref="WindowsTrayThemeProvider" /> class.
    /// </summary>
    public WindowsTrayThemeProvider(ILogger logger)
    {
        try
        {
            SystemEvents.UserPreferenceChanged += SystemEventsOnUserPreferenceChanged;
        }
        catch (Exception e)
        {
            logger.Warning(e, "Could not subscribe to system events, the tray icon won't follow changes to the taskbar color");
        }
    }

    /// <inheritdoc />
    public bool IsTrayDark
    {
        get
        {
            // Versions of Windows without the setting only have a dark taskbar
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(PersonalizeKey);
            return key?.GetValue("SystemUsesLightTheme") is not 1;
        }
    }

    /// <inheritdoc />
    public event EventHandler? TrayThemeChanged;

    private void SystemEventsOnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category == UserPreferenceCategory.General)
            TrayThemeChanged?.Invoke(this, EventArgs.Empty);
    }
}
