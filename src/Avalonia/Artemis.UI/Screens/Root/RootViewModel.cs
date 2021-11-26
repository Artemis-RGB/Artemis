using System;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.Root.Sidebar;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.Interfaces;
using Artemis.UI.Shared.Services.MainWindowService;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using Avalonia.Threading;
using ReactiveUI;

namespace Artemis.UI.Screens.Root
{
    public class RootViewModel : ActivatableViewModelBase, IScreen, IMainWindowProvider
    {
        private readonly IClassicDesktopStyleApplicationLifetime _lifeTime;
        private readonly ICoreService _coreService;
        private readonly ISettingsService _settingsService;
        private readonly IWindowService _windowService;
        private readonly IAssetLoader _assetLoader;
        private readonly ISidebarVmFactory _sidebarVmFactory;
        private SidebarViewModel? _sidebarViewModel;
        private TrayIcon? _trayIcon;
        private TrayIcons? _trayIcons;

        public RootViewModel(ICoreService coreService,
            ISettingsService settingsService,
            IRegistrationService registrationService,
            IWindowService windowService,
            IMainWindowService mainWindowService,
            IAssetLoader assetLoader,
            ISidebarVmFactory sidebarVmFactory)
        {
            Router = new RoutingState();

            _coreService = coreService;
            _settingsService = settingsService;
            _windowService = windowService;
            _assetLoader = assetLoader;
            _sidebarVmFactory = sidebarVmFactory;
            _lifeTime = (IClassicDesktopStyleApplicationLifetime) Application.Current.ApplicationLifetime;

            coreService.StartupArguments = _lifeTime.Args.ToList();
            mainWindowService.ConfigureMainWindowProvider(this);
            registrationService.RegisterProviders();
            
            DisplayAccordingToSettings();
            Task.Run(coreService.Initialize);
        }

        public SidebarViewModel? SidebarViewModel
        {
            get => _sidebarViewModel;
            set => this.RaiseAndSetIfChanged(ref _sidebarViewModel, value);
        }

        /// <inheritdoc />
        public RoutingState Router { get; }

        public async Task Exit()
        {
            // Don't freeze the UI right after clicking
            await Task.Delay(200);
            Utilities.Shutdown();
        }

        private void CurrentMainWindowOnClosed(object? sender, EventArgs e)
        {
            _lifeTime.MainWindow = null;
            SidebarViewModel = null;

            OnMainWindowClosed();
        }

        private void DisplayAccordingToSettings()
        {
            bool autoRunning = _coreService.StartupArguments.Contains("--autorun");
            bool minimized = _coreService.StartupArguments.Contains("--minimized");
            bool showOnAutoRun = _settingsService.GetSetting("UI.ShowOnStartup", true).Value;

            // Always show the tray icon if ShowOnStartup is false or the user has no way to open the main window
            bool showTrayIcon = !showOnAutoRun || _settingsService.GetSetting("UI.ShowTrayIcon", true).Value;

            if (showTrayIcon) 
                ShowTrayIcon();

            if (autoRunning && !showOnAutoRun || minimized)
            {
                // TODO: Auto-update
            }
            else
            {
                ShowSplashScreen();
                _coreService.Initialized += (_, _) => Dispatcher.UIThread.InvokeAsync(OpenMainWindow);
            }
        }

        private void ShowSplashScreen()
        {
            _windowService.ShowWindow<SplashViewModel>();
        }

        private void ShowTrayIcon()
        {
            _trayIcon = new TrayIcon {Icon = new WindowIcon(_assetLoader.Open(new Uri("avares://Artemis.UI/Assets/Images/Logo/bow.ico")))};
            _trayIcon.Menu = (NativeMenu?) Application.Current.FindResource("TrayIconMenu");
            _trayIcons = new TrayIcons {_trayIcon};
            TrayIcon.SetIcons(Application.Current, _trayIcons);
        }

        private void HideTrayIcon()
        {
            _trayIcon?.Dispose();
            TrayIcon.SetIcons(Application.Current, null!);

            _trayIcon = null;
            _trayIcons = null;
        }

        #region Implementation of IMainWindowProvider

        /// <inheritdoc />
        public bool IsMainWindowOpen => _lifeTime.MainWindow != null;

        /// <inheritdoc />
        public void OpenMainWindow()
        {
            if (_lifeTime.MainWindow == null)
            {
                SidebarViewModel = _sidebarVmFactory.SidebarViewModel(this);
                _lifeTime.MainWindow = new MainWindow {DataContext = this};
                _lifeTime.MainWindow.Show();
                _lifeTime.MainWindow.Closed += CurrentMainWindowOnClosed;
            }

            _lifeTime.MainWindow.Activate();

            OnMainWindowOpened();
        }

        /// <inheritdoc />
        public void CloseMainWindow()
        {
            _lifeTime.MainWindow?.Close();
        }

        /// <inheritdoc />
        public event EventHandler? MainWindowOpened;

        /// <inheritdoc />
        public event EventHandler? MainWindowClosed;

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