using System.Windows;
using System.Windows.Interop;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.Core.Services.Interfaces;
using Artemis.Core.Utilities;
using Artemis.UI.Events;
using Artemis.UI.Screens.Splash;
using Artemis.UI.Shared.Controls;
using Artemis.UI.Shared.Services.Interfaces;
using MaterialDesignExtensions.Controls;
using Ninject;
using Stylet;

namespace Artemis.UI.Screens
{
    public class TrayViewModel : PropertyChangedBase, IViewAware
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ICoreService _coreService;
        private readonly IKernel _kernel;
        private readonly IWindowManager _windowManager;
        private bool _setGradientPickerService;
        private SplashViewModel _splashViewModel;
        private bool _canShowRootViewModel;
        private UIElement _view;

        public TrayViewModel(IKernel kernel, IWindowManager windowManager, IEventAggregator eventAggregator, ICoreService coreService, ISettingsService settingsService)
        {
            _kernel = kernel;
            _windowManager = windowManager;
            _eventAggregator = eventAggregator;
            _coreService = coreService;
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

            // The gradient picker must have a reference to this service to be able to load saved gradients.
            // To avoid wasting resources, only set the service once and not until showing the UI.
            if (!_setGradientPickerService)
            {
                GradientPicker.GradientPickerService = _kernel.Get<IGradientPickerService>();
                _setGradientPickerService = true;
            }

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
            CurrentProcessUtilities.Shutdown(2, false);
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

        public void AttachView(UIElement view)
        {
            View = view;

            var handle = new WindowInteropHelper((Window) view).EnsureHandle();
            _coreService.SetMainWindowHandle(handle);
        }

        public UIElement View { get; set; }
    }
}