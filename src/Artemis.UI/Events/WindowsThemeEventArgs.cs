using System;
using Artemis.UI.Utilities;

namespace Artemis.UI.Events
{
    public class WindowsThemeEventArgs : EventArgs
    {
        public WindowsThemeEventArgs(ThemeWatcher.WindowsTheme theme)
        {
            Theme = theme;
        }

        public ThemeWatcher.WindowsTheme Theme { get; set; }
    }
}