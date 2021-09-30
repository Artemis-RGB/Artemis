using System;
using Artemis.UI.Services;
using Artemis.UI.Utilities;

namespace Artemis.UI.Events
{
    public class WindowsThemeEventArgs : EventArgs
    {
        public WindowsThemeEventArgs(IThemeService.WindowsTheme theme)
        {
            Theme = theme;
        }

        public IThemeService.WindowsTheme Theme { get; set; }
    }
}