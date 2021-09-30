using System;
using Artemis.UI.Events;

namespace Artemis.UI.Services
{
    public interface IThemeService : IArtemisUIService
    {
        WindowsTheme GetAppsTheme();
        WindowsTheme GetSystemTheme();
        event EventHandler<WindowsThemeEventArgs> AppsThemeChanged;
        event EventHandler<WindowsThemeEventArgs> SystemThemeChanged;

        enum WindowsTheme
        {
            Light,
            Dark
        }
    }
}