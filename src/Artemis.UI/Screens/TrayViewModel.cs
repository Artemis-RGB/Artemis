using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Artemis.Core.Services;
using Artemis.UI.Events;
using Artemis.UI.Screens.Splash;
using Artemis.UI.Services;
using Artemis.UI.Shared.Services;
using Hardcodet.Wpf.TaskbarNotification;
using Ninject;
using Stylet;

namespace Artemis.UI.Screens
{
    public class TrayViewModel : Screen, IMainWindowProvider
    {
        private readonly IDebugService _debugService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IKernel _kernel;
        private readonly IThemeService _themeService;
        private readonly IWindowManager _windowManager;
        private ImageSource _icon;
        private bool _openingMainWindow;
        private RootViewModel _rootViewModel;
        private SplashViewModel _splashViewModel;
        private TaskbarIcon _taskBarIcon;

        public TrayViewModel(IKernel kernel,
            IWindowManager windowManager,
            IWindowService windowService,
            IUpdateService updateService,
            IEventAggregator eventAggregator,
            ICoreService coreService,
            IDebugService debugService,
            ISettingsService settingsService,
            IThemeService themeService)
        {
            _kernel = kernel;
            _windowManager = windowManager;
            _eventAggregator = eventAggregator;
            _debugService = debugService;
            _themeService = themeService;

            Core.Utilities.ShutdownRequested += UtilitiesOnShutdownRequested;
            Core.Utilities.RestartRequested += UtilitiesOnShutdownRequested;

            _themeService.SystemThemeChanged += ThemeServiceOnSystemThemeChanged;
            ApplyTrayIconTheme(_themeService.GetSystemTheme());

            windowService.ConfigureMainWindowProvider(this);
            bool autoRunning = Bootstrapper.StartupArguments.Contains("--autorun");
            bool minimized = Bootstrapper.StartupArguments.Contains("--minimized");
            bool showOnAutoRun = settingsService.GetSetting("UI.ShowOnStartup", true).Value;

            if (autoRunning && !showOnAutoRun || minimized)
            {
                coreService.Initialized += (_, _) => updateService.AutoUpdate();
            }
            else
            {
                ShowSplashScreen();
                coreService.Initialized += (_, _) => TrayBringToForeground();
            }
        }

        public ImageSource Icon
        {
            get => _icon;
            set => SetAndNotify(ref _icon, value);
        }

        public void TrayBringToForeground()
        {
            if (_openingMainWindow)
                return;

            try
            {
                _openingMainWindow = true;

                if (IsMainWindowOpen)
                {
                    Execute.OnUIThreadSync(() =>
                    {
                        FocusMainWindow();
                        _openingMainWindow = false;
                    });
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

                    IsMainWindowOpen = true;
                    _openingMainWindow = false;
                });

                OnMainWindowOpened();
            }
            finally
            {
                _openingMainWindow = false;
            }
        }

        public void TrayActivateSidebarItem(string sidebarItem)
        {
            TrayBringToForeground();
            _eventAggregator.Publish(new RequestSelectSidebarItemEvent(sidebarItem));
        }

        public async Task TrayExit()
        {
            // Don't freeze the UI right after clicking
            await Task.Delay(200);
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
            lock (this)
            {
                if (_rootViewModel != null)
                {
                    _rootViewModel.Closed -= RootViewModelOnClosed;
                    _rootViewModel = null;
                }

                IsMainWindowOpen = false;
            }

            OnMainWindowClosed();
        }

        #region Theme

        private void ApplyTrayIconTheme(IThemeService.WindowsTheme theme)
        {
            Execute.PostToUIThread(() =>
            {
                Icon = theme == IThemeService.WindowsTheme.Dark
                    ? new BitmapImage(new Uri("pack://application:,,,/Artemis.UI;component/Resources/Images/Logo/bow-white.ico"))
                    : new BitmapImage(new Uri("pack://application:,,,/Artemis.UI;component/Resources/Images/Logo/bow-black.ico"));
            });
        }

        private void ThemeServiceOnSystemThemeChanged(object sender, WindowsThemeEventArgs e)
        {
            ApplyTrayIconTheme(e.Theme);
        }

        #endregion

        #region Implementation of IMainWindowProvider

        public bool IsMainWindowOpen { get; private set; }

        public bool OpenMainWindow()
        {
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
            MainWindowOpened?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnMainWindowClosed()
        {
            MainWindowClosed?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}