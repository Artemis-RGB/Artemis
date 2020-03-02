using System.Windows;
using Artemis.Core.Services;
using Artemis.Core.Services.Interfaces;
using Artemis.UI.Events;
using Artemis.UI.Screens.Splash;
using Ninject;
using Stylet;

namespace Artemis.UI.Screens
{
    public class TrayViewModel : Screen
    {
        private readonly IKernel _kernel;
        private readonly IWindowManager _windowManager;
        private readonly IEventAggregator _eventAggregator;
        private SplashViewModel _splashViewModel;

        public TrayViewModel(IKernel kernel, IWindowManager windowManager, IEventAggregator eventAggregator, ICoreService coreService, ISettingsService settingsService)
        {
            _kernel = kernel;
            _windowManager = windowManager;
            _eventAggregator = eventAggregator;
            CanShowRootViewModel = true;

            var autoRunning = Bootstrapper.StartupArguments.Contains("-autorun");
            var showOnAutoRun = settingsService.GetSetting("UI.ShowOnStartup", true).Value;
            if (!autoRunning || showOnAutoRun)
            {
                ShowSplashScreen();
                coreService.Initialized += (sender, args) => TrayBringToForeground();
            }
        }

        public bool CanShowRootViewModel { get; set; }

        public void TrayBringToForeground()
        {
            if (!CanShowRootViewModel)
                return;

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
            Application.Current.Shutdown();
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