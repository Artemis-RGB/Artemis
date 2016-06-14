using System;
using System.Threading.Tasks;
using System.Windows;
using Artemis.Events;
using Artemis.Managers;
using Artemis.Services;
using Artemis.Settings;
using Artemis.Utilities;
using Caliburn.Micro;
using Ninject;

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

        public SystemTrayViewModel(IWindowManager windowManager, IEventAggregator events, ShellViewModel shellViewModel,
            MainManager mainManager)
        {
            _windowManager = windowManager;
            _shellViewModel = shellViewModel;
            _checkedForUpdate = false;

            MainManager = mainManager;

            events.Subscribe(this);
            MainManager.EnableProgram();

            if (General.Default.ShowOnStartup)
                ShowWindow();
        }

        [Inject]
        public MetroDialogService DialogService { get; set; }

        public MainManager MainManager { get; set; }

        public bool CanShowWindow => !_shellViewModel.IsActive;

        public bool CanHideWindow => _shellViewModel.IsActive && !MainManager.DeviceManager.ChangingKeyboard;
        public bool CanToggleEnabled => !MainManager.DeviceManager.ChangingKeyboard;

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
                MainManager.DisableProgram();
            else
                MainManager.EnableProgram();
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

            ShowKeyboardDialog();
            Updater.CheckForUpdate(DialogService);
        }

        private async void ShowKeyboardDialog()
        {
            NotifyOfPropertyChange(() => CanHideWindow);
            NotifyOfPropertyChange(() => CanToggleEnabled);

            var dialog = await DialogService.ShowProgressDialog("Enabling keyboard",
                "Artemis is still busy trying to enable your last used keyboard. " +
                "Please what while the progress completes");
            dialog.SetIndeterminate();

            while (MainManager.DeviceManager.ChangingKeyboard)
                await Task.Delay(200);

            NotifyOfPropertyChange(() => CanHideWindow);
            NotifyOfPropertyChange(() => CanToggleEnabled);

            try
            {
                await dialog.CloseAsync();
            }
            catch (InvalidOperationException)
            {
                // Occurs when window is closed again, can't find a proper check for this
            }
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
            MainManager.Dispose();
            Application.Current.Shutdown();
        }
    }
}