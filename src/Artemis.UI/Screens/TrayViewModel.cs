using System.Windows;
using Artemis.Core.Services;
using Artemis.Core.Services.Interfaces;
using Artemis.UI.Screens.Splash;
using Ninject;
using Stylet;

namespace Artemis.UI.Screens
{
    public class TrayViewModel : Screen
    {
        private readonly ICoreService _coreService;
        private readonly IKernel _kernel;
        private readonly IWindowManager _windowManager;

        public TrayViewModel(IKernel kernel, IWindowManager windowManager, ICoreService coreService, ISettingsService settingsService)
        {
            _kernel = kernel;
            _windowManager = windowManager;
            _coreService = coreService;
            CanShowRootViewModel = true;

            var autoRunning = Bootstrapper.StartupArguments.Contains("-autorun");
            var showOnAutoRun = settingsService.GetSetting("UI.ShowOnStartup", true).Value;
            if (!autoRunning || showOnAutoRun)
            {
                ShowSplashScreen();
                _coreService.Initialized += (sender, args) => TrayBringToForeground();
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
                var rootViewModel = _kernel.Get<RootViewModel>();
                rootViewModel.Closed += RootViewModelOnClosed;
                _windowManager.ShowWindow(rootViewModel);
            });
        }

        public void TrayExit()
        {
            Application.Current.Shutdown();
        }

        private void ShowSplashScreen()
        {
            Execute.OnUIThread(() =>
            {
                var splashViewModel = _kernel.Get<SplashViewModel>();
                _windowManager.ShowWindow(splashViewModel);
            });
        }

        private void RootViewModelOnClosed(object sender, CloseEventArgs e)
        {
            CanShowRootViewModel = true;
        }
    }
}