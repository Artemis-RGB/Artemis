using System;
using Artemis.Core.Services;
using Artemis.UI.Events;
using Artemis.UI.Screens.Splash;
using Artemis.UI.Services;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Shared.Services;
using Ninject;
using Stylet;

namespace Artemis.UI.Screens
{
    public class TrayViewModel : Screen, IMainWindowManager
    {
        private readonly IDebugService _debugService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IKernel _kernel;
        private readonly IWindowManager _windowManager;
        private bool _canShowRootViewModel;
        private SplashViewModel _splashViewModel;
        private RootViewModel _rootViewModel;

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
            CanShowRootViewModel = true;

            windowService.ConfigureMainWindowManager(this);
            bool autoRunning = Bootstrapper.StartupArguments.Contains("--autorun");
            bool showOnAutoRun = settingsService.GetSetting("UI.ShowOnStartup", true).Value;
            if (!autoRunning || showOnAutoRun)
            {
                ShowSplashScreen();
                coreService.Initialized += (_, _) => TrayBringToForeground();
            }

            updateService.AutoUpdate();
        }

        public bool CanShowRootViewModel
        {
            get => _canShowRootViewModel;
            set => SetAndNotify(ref _canShowRootViewModel, value);
        }

        public void TrayBringToForeground()
        {
            if (!CanShowRootViewModel)
                return;

            // Initialize the shared UI when first showing the window
            if (!UI.Shared.Bootstrapper.Initialized)
                UI.Shared.Bootstrapper.Initialize(_kernel);

            CanShowRootViewModel = false;
            Execute.OnUIThread(() =>
            {
                _splashViewModel?.RequestClose();
                _splashViewModel = null;
                _rootViewModel = _kernel.Get<RootViewModel>();
                _rootViewModel.Closed += RootViewModelOnClosed;
                _windowManager.ShowWindow(_rootViewModel);
            });

            OnMainWindowOpened();
        }

        public void TrayActivateSidebarItem(string sidebarItem)
        {
            TrayBringToForeground();
            _eventAggregator.Publish(new RequestSelectSidebarItemEvent(sidebarItem));
        }

        public void TrayExit()
        {
            Core.Utilities.Shutdown(2, false);
        }

        public void TrayOpenDebugger()
        {
            _debugService.ShowDebugger();
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
            _rootViewModel.Closed -= RootViewModelOnClosed;
            _rootViewModel = null;

            CanShowRootViewModel = true;
            OnMainWindowClosed();
        }

        #region Implementation of IMainWindowManager

        /// <inheritdoc />
        public bool IsMainWindowOpen { get; private set; }

        /// <inheritdoc />
        public bool OpenMainWindow()
        {
            if (CanShowRootViewModel)
                return false;

            TrayBringToForeground();
            return true;
        }

        /// <inheritdoc />
        public bool CloseMainWindow()
        {
            _rootViewModel.RequestClose();
            return _rootViewModel.ScreenState == ScreenState.Closed;
        }

        /// <inheritdoc />
        public event EventHandler MainWindowOpened;

        /// <inheritdoc />
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