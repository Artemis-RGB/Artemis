using System;
using System.Globalization;
using System.Management;
using System.Security.Principal;
using Artemis.UI.Events;
using Microsoft.Win32;

namespace Artemis.UI.Utilities
{
    public class ThemeWatcher
    {
        private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

        private const string appsThemeRegistryValueName = "AppsUseLightTheme";
        private const string systemThemeRegistryValueName = "SystemUsesLightTheme";

        public ThemeWatcher()
        {
            WatchTheme();
        }

        public void WatchTheme()
        {
            WindowsIdentity currentUser = WindowsIdentity.GetCurrent();
            string appsThemequery = string.Format(
                CultureInfo.InvariantCulture,
                @"SELECT * FROM RegistryValueChangeEvent WHERE Hive = 'HKEY_USERS' AND KeyPath = '{0}\\{1}' AND ValueName = '{2}'",
                currentUser.User.Value,
                RegistryKeyPath.Replace(@"\", @"\\"),
                appsThemeRegistryValueName);

            string systemThemequery = string.Format(
                CultureInfo.InvariantCulture,
                @"SELECT * FROM RegistryValueChangeEvent WHERE Hive = 'HKEY_USERS' AND KeyPath = '{0}\\{1}' AND ValueName = '{2}'",
                currentUser.User.Value,
                RegistryKeyPath.Replace(@"\", @"\\"),
                systemThemeRegistryValueName);

            try
            {
                // For Apps theme
                ManagementEventWatcher appsThemWatcher = new(appsThemequery);
                appsThemWatcher.EventArrived += (_, _) =>
                {
                    WindowsTheme newWindowsTheme = GetAppsTheme();
                    OnAppsThemeChanged(new WindowsThemeEventArgs(newWindowsTheme));
                };

                // Start listening for apps theme events
                appsThemWatcher.Start();


                // For System theme
                ManagementEventWatcher systemThemWatcher = new(systemThemequery);
                systemThemWatcher.EventArrived += (_, _) =>
                {
                    WindowsTheme newWindowsTheme = GetSystemTheme();
                    OnSystemThemeChanged(new WindowsThemeEventArgs(newWindowsTheme));
                };

                // Start listening for system theme events
                systemThemWatcher.Start();
            }
            catch (Exception)
            {
                // This can fail on Windows 7
            }
        }

        private WindowsTheme GetTheme(string themeKeyName)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath))
            {
                object registryValueObject = key?.GetValue(themeKeyName);
                if (registryValueObject == null) return WindowsTheme.Light;

                int registryValue = (int)registryValueObject;

                return registryValue > 0 ? WindowsTheme.Light : WindowsTheme.Dark;
            }
        }

        public WindowsTheme GetAppsTheme()
        {
            return GetTheme(appsThemeRegistryValueName);
        }

        public WindowsTheme GetSystemTheme()
        {
            return GetTheme(systemThemeRegistryValueName);
        }

        public event EventHandler<WindowsThemeEventArgs> AppsThemeChanged;
        public event EventHandler<WindowsThemeEventArgs> SystemThemeChanged;

        protected virtual void OnAppsThemeChanged(WindowsThemeEventArgs e)
        {
            AppsThemeChanged?.Invoke(this, e);
        }

        protected virtual void OnSystemThemeChanged(WindowsThemeEventArgs e)
        {
            SystemThemeChanged?.Invoke(this, e);
        }

        public enum WindowsTheme
        {
            Light,
            Dark
        }
    }
}