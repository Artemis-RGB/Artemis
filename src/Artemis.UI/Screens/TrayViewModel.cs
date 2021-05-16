using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Events;
using Artemis.UI.Screens.Settings.Tabs.General;
using Artemis.UI.Screens.Splash;
using Artemis.UI.Services;
using Artemis.UI.Shared.Services;
using Artemis.UI.Utilities;
using Hardcodet.Wpf.TaskbarNotification;
using MaterialDesignThemes.Wpf;
using Ninject;
using Stylet;

namespace Artemis.UI.Screens
{
    public class TrayViewModel : Screen, IMainWindowProvider
    {
        private readonly PluginSetting<ApplicationColorScheme> _colorScheme;
        private readonly IDebugService _debugService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IKernel _kernel;
        private readonly ThemeWatcher _themeWatcher;
        private readonly IWindowManager _windowManager;
        private RootViewModel _rootViewModel;
        private SplashViewModel _splashViewModel;
        private TaskbarIcon _taskBarIcon;
        private ImageSource _icon;

        public TrayViewModel(IKernel kernel,
            IWindowManager windowManager,
            IWindowService windowService,
            IUpdateService updateService,
            IEventAggregator eventAggregator,
            ICoreService coreService,
            IDebugService debugService,
            ISettingsService settingsService)
        {
            _kernel = kernel;
            _windowManager = windowManager;
            _eventAggregator = eventAggregator;
            _debugService = debugService;

            Core.Utilities.ShutdownRequested += UtilitiesOnShutdownRequested;
            Core.Utilities.RestartRequested += UtilitiesOnShutdownRequested;

            _themeWatcher = new ThemeWatcher();
            _colorScheme = settingsService.GetSetting("UI.ColorScheme", ApplicationColorScheme.Automatic);
            _colorScheme.SettingChanged += ColorSchemeOnSettingChanged;
            _themeWatcher.SystemThemeChanged += _themeWatcher_SystemThemeChanged;
            _themeWatcher.AppsThemeChanged += _themeWatcher_AppsThemeChanged;

            ApplyColorSchemeSetting();
            ApplyTryIconTheme(_themeWatcher.GetSystemTheme());

            windowService.ConfigureMainWindowProvider(this);
            bool autoRunning = Bootstrapper.StartupArguments.Contains("--autorun");
            bool minimized = Bootstrapper.StartupArguments.Contains("--minimized");
            bool showOnAutoRun = settingsService.GetSetting("UI.ShowOnStartup", true).Value;

            if (autoRunning && !showOnAutoRun || minimized)
                coreService.Initialized += (_, _) => updateService.AutoUpdate();
            else
            {
                ShowSplashScreen();
                coreService.Initialized += (_, _) => TrayBringToForeground();
            }
        }

        public void TrayBringToForeground()
        {
            if (IsMainWindowOpen)
            {
                Execute.PostToUIThread(FocusMainWindow);
                return;
            }

            // Initialize the shared UI when first showing the window
            if (!UI.Shared.Bootstrapper.Initialized)
                UI.Shared.Bootstrapper.Initialize(_kernel);

            Execute.OnUIThreadSync(() =>
            {
                _splashViewModel?.RequestClose();
                _splashViewModel = null;
                _rootViewModel = _kernel.Get<RootViewModel>();
                _rootViewModel.Closed += RootViewModelOnClosed;
                _windowManager.ShowWindow(_rootViewModel);
            });

            OnMainWindowOpened();
        }

        public ImageSource Icon
        {
            get => _icon;
            set => SetAndNotify(ref _icon, value);
        }

        public void TrayActivateSidebarItem(string sidebarItem)
        {
            TrayBringToForeground();
            _eventAggregator.Publish(new RequestSelectSidebarItemEvent(sidebarItem));
        }

        public void TrayExit()
        {
            Core.Utilities.Shutdown();
        }

        public void TrayOpenDebugger()
        {
            _debugService.ShowDebugger();
        }

        public void SetTaskbarIcon(UIElement view)
        {
            _taskBarIcon = (TaskbarIcon) ((ContentControl) view).Content;
        }

        public void OnTrayBalloonTipClicked(object sender, EventArgs e)
        {
            if (!IsMainWindowOpen)
                TrayBringToForeground();
            else
                FocusMainWindow();
        }

        private void FocusMainWindow()
        {
            // Wrestle the main window to the front
            Window mainWindow = (Window) _rootViewModel.View;
            if (mainWindow.WindowState == WindowState.Minimized)
                mainWindow.WindowState = WindowState.Normal;
            mainWindow.Activate();
            mainWindow.Topmost = true;
            mainWindow.Topmost = false;
            mainWindow.Focus();
        }

        private void UtilitiesOnShutdownRequested(object sender, EventArgs e)
        {
            Execute.OnUIThread(() => _taskBarIcon?.Dispose());
        }

        private void ShowSplashScreen()
        {
            Execute.OnUIThread(() =>
            {
                _splashViewModel = _kernel.Get<SplashViewModel>();
                _windowManager.ShowWindow(_splashViewModel);
            });
        }

        private void RootViewModelOnClosed(object sender, CloseEventArgs e)
        {
            if (_rootViewModel != null)
            {
                _rootViewModel.Closed -= RootViewModelOnClosed;
                _rootViewModel = null;
            }

            OnMainWindowClosed();
        }

        #region Theme

        private void ApplyColorSchemeSetting()
        {
            if (_colorScheme.Value == ApplicationColorScheme.Automatic)
                ApplyUITheme(_themeWatcher.GetAppsTheme());
            else
                ChangeMaterialColors(_colorScheme.Value);
        }

        private void ApplyUITheme(ThemeWatcher.WindowsTheme theme)
        {
            if (_colorScheme.Value != ApplicationColorScheme.Automatic)
                return;
            if (theme == ThemeWatcher.WindowsTheme.Dark)
                ChangeMaterialColors(ApplicationColorScheme.Dark);
            else
                ChangeMaterialColors(ApplicationColorScheme.Light);
        }

        private void ApplyTryIconTheme(ThemeWatcher.WindowsTheme theme)
        {
            Execute.PostToUIThread(() =>
            {
                Icon = theme == ThemeWatcher.WindowsTheme.Dark
                    ? new BitmapImage(new Uri("pack://application:,,,/Artemis.UI;component/Resources/Images/Logo/bow-white.ico"))
                    : new BitmapImage(new Uri("pack://application:,,,/Artemis.UI;component/Resources/Images/Logo/bow-black.ico"));
            });
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

        private void _themeWatcher_AppsThemeChanged(object sender, WindowsThemeEventArgs e)
        {
            ApplyUITheme(e.Theme);
        }

        private void _themeWatcher_SystemThemeChanged(object sender, WindowsThemeEventArgs e)
        {
            ApplyTryIconTheme(e.Theme);
        }

        private void ColorSchemeOnSettingChanged(object sender, EventArgs e)
        {
            ApplyColorSchemeSetting();
        }

        #endregion

        #region Implementation of IMainWindowProvider

        public bool IsMainWindowOpen { get; private set; }

        public bool OpenMainWindow()
        {
            if (IsMainWindowOpen)
                Execute.OnUIThread(FocusMainWindow);
            else
                TrayBringToForeground();
            return _rootViewModel.ScreenState == ScreenState.Active;
        }

        public bool CloseMainWindow()
        {
            Execute.OnUIThread(() => _rootViewModel.RequestClose());
            return _rootViewModel.ScreenState == ScreenState.Closed;
        }

        public event EventHandler MainWindowOpened;

        public event EventHandler MainWindowClosed;

        protected virtual void OnMainWindowOpened()
        {
            IsMainWindowOpen = true;
            MainWindowOpened?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnMainWindowClosed()
        {
            IsMainWindowOpen = false;
            MainWindowClosed?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}