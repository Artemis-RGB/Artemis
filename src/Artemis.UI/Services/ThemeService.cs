using System;
using System.Globalization;
using System.Management;
using System.Security.Principal;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Events;
using Artemis.UI.Screens.Settings.Tabs.General;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Serilog;

namespace Artemis.UI.Services
{
    public class ThemeService : IThemeService
    {
        private readonly ILogger _logger;
        private readonly PluginSetting<ApplicationColorScheme> _colorScheme;
        private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

        private const string AppsThemeRegistryValueName = "AppsUseLightTheme";
        private const string SystemThemeRegistryValueName = "SystemUsesLightTheme";

        public ThemeService(ISettingsService settingsService, ILogger logger)
        {
            _logger = logger;
            _colorScheme = settingsService.GetSetting("UI.ColorScheme", ApplicationColorScheme.Automatic);
            _colorScheme.SettingChanged += ColorSchemeOnSettingChanged;

            AppsThemeChanged += OnAppsThemeChanged;

            Task.Run(Initialize);
        }

        private void Initialize()
        {
            try
            {
                WatchTheme();
            }
            catch (Exception e)
            {
                _logger.Warning(e, "WatchTheme failed");
            }

            try
            {
                ApplyColorSchemeSetting();
            }
            catch (Exception e)
            {
                _logger.Warning(e, "ApplyColorSchemeSetting failed");
            }
        }

        public IThemeService.WindowsTheme GetAppsTheme()
        {
            return GetTheme(AppsThemeRegistryValueName);
        }

        public IThemeService.WindowsTheme GetSystemTheme()
        {
            return GetTheme(SystemThemeRegistryValueName);
        }

        private void ApplyColorSchemeSetting()
        {
            if (_colorScheme.Value == ApplicationColorScheme.Automatic)
                ApplyUITheme(GetAppsTheme());
            else
                ChangeMaterialColors(_colorScheme.Value);
        }

        private void ChangeMaterialColors(ApplicationColorScheme colorScheme)
        {
            PaletteHelper paletteHelper = new();
            ITheme theme = paletteHelper.GetTheme();
            theme.SetBaseTheme(colorScheme == ApplicationColorScheme.Dark ? Theme.Dark : Theme.Light);
            paletteHelper.SetTheme(theme);

            MaterialDesignExtensions.Themes.PaletteHelper extensionsPaletteHelper = new();
            extensionsPaletteHelper.SetLightDark(colorScheme == ApplicationColorScheme.Dark);
        }

        private void ApplyUITheme(IThemeService.WindowsTheme theme)
        {
            if (_colorScheme.Value != ApplicationColorScheme.Automatic)
                return;
            if (theme == IThemeService.WindowsTheme.Dark)
                ChangeMaterialColors(ApplicationColorScheme.Dark);
            else
                ChangeMaterialColors(ApplicationColorScheme.Light);
        }

        private void WatchTheme()
        {
            WindowsIdentity currentUser = WindowsIdentity.GetCurrent();
            string appsThemequery = string.Format(
                CultureInfo.InvariantCulture,
                @"SELECT * FROM RegistryValueChangeEvent WHERE Hive = 'HKEY_USERS' AND KeyPath = '{0}\\{1}' AND ValueName = '{2}'",
                currentUser.User.Value,
                RegistryKeyPath.Replace(@"\", @"\\"),
                AppsThemeRegistryValueName);

            string systemThemequery = string.Format(
                CultureInfo.InvariantCulture,
                @"SELECT * FROM RegistryValueChangeEvent WHERE Hive = 'HKEY_USERS' AND KeyPath = '{0}\\{1}' AND ValueName = '{2}'",
                currentUser.User.Value,
                RegistryKeyPath.Replace(@"\", @"\\"),
                SystemThemeRegistryValueName);

            try
            {
                // For Apps theme
                ManagementEventWatcher appsThemWatcher = new(appsThemequery);
                appsThemWatcher.EventArrived += (_, _) =>
                {
                    IThemeService.WindowsTheme newWindowsTheme = GetAppsTheme();
                    OnAppsThemeChanged(new WindowsThemeEventArgs(newWindowsTheme));
                };

                // Start listening for apps theme events
                appsThemWatcher.Start();


                // For System theme
                ManagementEventWatcher systemThemWatcher = new(systemThemequery);
                systemThemWatcher.EventArrived += (_, _) =>
                {
                    IThemeService.WindowsTheme newWindowsTheme = GetSystemTheme();
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

        private IThemeService.WindowsTheme GetTheme(string themeKeyName)
        {
            using RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
            object registryValueObject = key?.GetValue(themeKeyName);
            if (registryValueObject == null) return IThemeService.WindowsTheme.Light;

            int registryValue = (int)registryValueObject;

            return registryValue > 0 ? IThemeService.WindowsTheme.Light : IThemeService.WindowsTheme.Dark;
        }

        #region Events

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

        #endregion

        #region Event handlers

        private void ColorSchemeOnSettingChanged(object sender, EventArgs e)
        {
            ApplyColorSchemeSetting();
        }

        private void OnAppsThemeChanged(object sender, WindowsThemeEventArgs e)
        {
            ApplyUITheme(e.Theme);
        }

        #endregion
    }
}