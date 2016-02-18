using System.Windows;
using Caliburn.Micro;

namespace Artemis.ViewModels
{
    public class SystemTrayViewModel : Screen
    {
        private readonly ShellViewModel _shellViewModel;

        private readonly IWindowManager _windowManager;
        /*
         * NOTE: In this sample the system tray view-model doesn't receive any notification 
         * when the other window gets closed by pressing the top right 'x'.
         * Thus no property notification is invoked, and system tray context-menu appears 
         * out of sync, still allowing 'Hide' and disabling 'Show'.
         * Given the purpose of the sample - integrating Caliburn.Micro with WPF NotifyIcon -
         * sync'ing the two view-models is not of interest here.
         * */

        public SystemTrayViewModel(IWindowManager windowManager, ShellViewModel shellViewModel)
        {
            _windowManager = windowManager;
            _shellViewModel = shellViewModel;

            // TODO: Check if show on startup is enabled, if so, show window.
        }

        public bool CanShowWindow => !_shellViewModel.IsActive;

        public bool CanHideWindow => _shellViewModel.IsActive;

        protected override void OnActivate()
        {
            base.OnActivate();

            NotifyOfPropertyChange(() => CanShowWindow);
            NotifyOfPropertyChange(() => CanHideWindow);
        }

        public void ShowWindow()
        {
            if (!CanShowWindow)
                return;

            // manually show the next window view-model
            _windowManager.ShowWindow(_shellViewModel);

            NotifyOfPropertyChange(() => CanShowWindow);
            NotifyOfPropertyChange(() => CanHideWindow);
        }

        public void HideWindow()
        {
            if (!CanHideWindow)
                return;

            _shellViewModel.TryClose();

            NotifyOfPropertyChange(() => CanShowWindow);
            NotifyOfPropertyChange(() => CanHideWindow);
        }

        public void ExitApplication()
        {
            _shellViewModel.MainModel.ShutdownEffects();
            Application.Current.Shutdown();
        }
    }
}