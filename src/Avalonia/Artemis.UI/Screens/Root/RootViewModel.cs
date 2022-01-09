
using System;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.Sidebar;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.Interfaces;
using Artemis.UI.Shared.Services.MainWindow;
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
        private readonly IAssetLoader _assetLoader;
        private readonly DefaultTitleBarViewModel _defaultTitleBarViewModel;
        private readonly ICoreService _coreService;
        private readonly IDebugService _debugService;
        private readonly IClassicDesktopStyleApplicationLifetime _lifeTime;
        private readonly ISettingsService _settingsService;
        private readonly ISidebarVmFactory _sidebarVmFactory;
        private readonly IWindowService _windowService;
        private SidebarViewModel? _sidebarViewModel;
        private TrayIcon? _trayIcon;
        private TrayIcons? _trayIcons;
        private ViewModelBase? _titleBarViewModel;

        public RootViewModel(ICoreService coreService,
            ISettingsService settingsService,
            IRegistrationService registrationService,
            IWindowService windowService,
            IMainWindowService mainWindowService,
            IDebugService debugService,
            IAssetLoader assetLoader,
            DefaultTitleBarViewModel defaultTitleBarViewModel,
            ISidebarVmFactory sidebarVmFactory)
        {
            Router = new RoutingState();

            _coreService = coreService;
            _settingsService = settingsService;
            _windowService = windowService;
            _debugService = debugService;
            _assetLoader = assetLoader;
            _defaultTitleBarViewModel = defaultTitleBarViewModel;
            _sidebarVmFactory = sidebarVmFactory;
            _lifeTime = (IClassicDesktopStyleApplicationLifetime) Application.Current!.ApplicationLifetime!;

            coreService.StartupArguments = _lifeTime.Args.ToList();
            mainWindowService.ConfigureMainWindowProvider(this);

            DisplayAccordingToSettings();
            Router.CurrentViewModel.Subscribe(UpdateTitleBarViewModel);
            Task.Run(coreService.Initialize);
        }

        private void UpdateTitleBarViewModel(IRoutableViewModel? viewModel)
        {
            if (viewModel is MainScreenViewModel mainScreenViewModel && mainScreenViewModel.TitleBarViewModel != null)
                TitleBarViewModel = mainScreenViewModel.TitleBarViewModel;
            else
                TitleBarViewModel = _defaultTitleBarViewModel;
        }


        public SidebarViewModel? SidebarViewModel
        {
            get => _sidebarViewModel;
            set => this.RaiseAndSetIfChanged(ref _sidebarViewModel, value);
        }

        public ViewModelBase? TitleBarViewModel
        {
            get => _titleBarViewModel;
            set => this.RaiseAndSetIfChanged(ref _titleBarViewModel, value);
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
            _trayIcon = new TrayIcon
            {
                Icon = new WindowIcon(_assetLoader.Open(new Uri("avares://Artemis.UI/Assets/Images/Logo/bow.ico"))),
                Command = ReactiveCommand.Create(OpenMainWindow)
            };
            _trayIcon.Menu = (NativeMenu?) Application.Current!.FindResource("TrayIconMenu");
            _trayIcons = new TrayIcons {_trayIcon};
            TrayIcon.SetIcons(Application.Current!, _trayIcons);
        }

        private void HideTrayIcon()
        {
            _trayIcon?.Dispose();
            TrayIcon.SetIcons(Application.Current!, null!);

            _trayIcon = null;
            _trayIcons = null;
        }

        /// <inheritdoc />
        public RoutingState Router { get; }

        #region Tray commands

        public void OpenScreen(string displayName)
        {
            OpenMainWindow();

            // At this point there is a sidebar VM because the main window was opened
            SidebarViewModel!.SelectedSidebarScreen = SidebarViewModel.SidebarScreens.FirstOrDefault(s => s.DisplayName == displayName);
        }

        public async Task OpenDebugger()
        {
            await Dispatcher.UIThread.InvokeAsync(() => _debugService.ShowDebugger());
        }

        public async Task Exit()
        {
            // Don't freeze the UI right after clicking
            await Task.Delay(200);
            Utilities.Shutdown();
        }

        #endregion

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

            _lifeTime.MainWindow.WindowState = WindowState.Normal;
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