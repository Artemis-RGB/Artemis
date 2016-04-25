using System;
using System.Windows;
using Artemis.Events;
using Artemis.Properties;
using Artemis.Settings;
using Artemis.Utilities;
using Caliburn.Micro;

namespace Artemis.ViewModels
{
    public class SystemTrayViewModel : Screen, IHandle<ToggleEnabled>
    {
        private readonly ShellViewModel _shellViewModel;

        private readonly IWindowManager _windowManager;
        private string _activeIcon;
        private bool _checkedForUpdate;
        private bool _enabled;
        private string _toggleText;

        public SystemTrayViewModel(IWindowManager windowManager, ShellViewModel shellViewModel)
        {
            _windowManager = windowManager;
            _shellViewModel = shellViewModel;
            _shellViewModel.MainManager.Events.Subscribe(this);
            _shellViewModel.MainManager.EnableProgram();
            _checkedForUpdate = false;

            if (General.Default.ShowOnStartup)
                ShowWindow();
        }

        public bool CanShowWindow => !_shellViewModel.IsActive;

        public bool CanHideWindow => _shellViewModel.IsActive;

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (value == _enabled) return;
                _enabled = value;

                ToggleText = _enabled ? "Disable Artemis" : "Enable Artemis";
                ActiveIcon = _enabled ? "../Resources/logo.ico" : "../Resources/logo-disabled.ico";
                NotifyOfPropertyChange(() => Enabled);
            }
        }

        public string ActiveIcon
        {
            get { return _activeIcon; }
            set
            {
                _activeIcon = value;
                NotifyOfPropertyChange();
            }
        }

        public string ToggleText
        {
            get { return _toggleText; }
            set
            {
                if (value == _toggleText) return;
                _toggleText = value;
                NotifyOfPropertyChange(() => ToggleText);
            }
        }

        public void Handle(ToggleEnabled message)
        {
            Enabled = message.Enabled;
        }

        public void ToggleEnabled()
        {
            if (Enabled)
                _shellViewModel.MainManager.DisableProgram();
            else
                _shellViewModel.MainManager.EnableProgram();
        }

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

            if (_checkedForUpdate)
                return;

            _checkedForUpdate = true;
            Updater.CheckForUpdate(_shellViewModel.MainManager.DialogService);
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
            _shellViewModel.MainManager.Shutdown();
            Application.Current.Shutdown();

            // Sometimes you need to be rough.
            Environment.Exit(0);
        }
    }
}