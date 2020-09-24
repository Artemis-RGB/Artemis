using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Events;
using Artemis.UI.Screens.Splash;
using Artemis.UI.Services.Interfaces;
using Ninject;
using Stylet;

namespace Artemis.UI.Screens
{
    public class TrayViewModel : Screen
    {
        private readonly IDebugService _debugService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IKernel _kernel;
        private readonly IWindowManager _windowManager;
        private bool _canShowRootViewModel;
        private SplashViewModel _splashViewModel;

        public TrayViewModel(IKernel kernel, IWindowManager windowManager, IEventAggregator eventAggregator, ICoreService coreService, IDebugService debugService, ISettingsService settingsService)
        {
            _kernel = kernel;
            _windowManager = windowManager;
            _eventAggregator = eventAggregator;
            _debugService = debugService;
            CanShowRootViewModel = true;

            var autoRunning = Bootstrapper.StartupArguments.Contains("--autorun");
            var showOnAutoRun = settingsService.GetSetting("UI.ShowOnStartup", true).Value;
            if (!autoRunning || showOnAutoRun)
            {
                ShowSplashScreen();
                coreService.Initialized += (sender, args) => TrayBringToForeground();
            }
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
                var rootViewModel = _kernel.Get<RootViewModel>();
                rootViewModel.Closed += RootViewModelOnClosed;
                _windowManager.ShowWindow(rootViewModel);
            });
        }

        public void TrayActivateSidebarItem(string sidebarItem)
        {
            TrayBringToForeground();
            _eventAggregator.Publish(new RequestSelectSidebarItemEvent(sidebarItem));
        }

        public void TrayExit()
        {
            ApplicationUtilities.Shutdown(2, false);
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
            CanShowRootViewModel = true;
        }
    }
}