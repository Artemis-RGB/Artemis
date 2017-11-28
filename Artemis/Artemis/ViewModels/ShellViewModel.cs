using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.DAL;
using Artemis.Managers;
using Artemis.Services;
using Artemis.Settings;
using Artemis.Utilities;
using Artemis.ViewModels.Abstract;
using Artemis.ViewModels.Flyouts;
using Caliburn.Micro;
using Hardcodet.Wpf.TaskbarNotification;
using MahApps.Metro.Controls;
using Ninject;

namespace Artemis.ViewModels
{
    public sealed class ShellViewModel : Conductor<IScreen>.Collection.OneActive
    {
        private readonly IKernel _kernel;
        private string _activeIcon;
        private bool _checkedForDuplicate;
        private bool _enabled;
        private bool _exiting;
        private string _toggleText;

        public ShellViewModel(IKernel kernel, MainManager mainManager, MetroDialogService metroDialogService, FlyoutSettingsViewModel flyoutSettings)
        {
            _kernel = kernel;
            var checkedForUpdate = false;

            MainManager = mainManager;
            MetroDialogService = metroDialogService;

            DisplayName = "Artemis";
            ActiveIcon = "../Resources/logo-disabled.ico";
            GeneralSettings = SettingsProvider.Load<GeneralSettings>();
            Flyouts = new BindableCollection<FlyoutBaseViewModel>
            {
                flyoutSettings
            };

            MainManager.EnabledChanged += (sender, args) =>
            {
                Enabled = args.Enabled;
                // Check for updates once if auto update is enabled and the window is shown
                if (GeneralSettings.AutoUpdate && !checkedForUpdate && CanHideWindow)
                {
                    checkedForUpdate = true;
                    Updater.CheckForUpdate(MetroDialogService);
                }
            };


            // This gets updated automatically but during startup lets quickly preset it
            Enabled = GeneralSettings.Suspended;
        }

        public Mutex Mutex { get; set; }
        public MainManager MainManager { get; set; }
        public MetroDialogService MetroDialogService { get; set; }
        public IObservableCollection<FlyoutBaseViewModel> Flyouts { get; set; }
        public GeneralSettings GeneralSettings { get; set; }
        private MetroWindow Window => (MetroWindow) GetView();

        public bool CanShowWindow => Window != null && (Window != null || !Window.IsVisible);
        public bool CanHideWindow => Window != null && Window.IsVisible;

        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (value == _enabled)
                    return;
                _enabled = value;

                ToggleText = _enabled ? "Disable Artemis" : "Enable Artemis";
                ActiveIcon = _enabled ? "../Resources/logo.ico" : "../Resources/logo-disabled.ico";
                NotifyOfPropertyChange(() => Enabled);
            }
        }

        public string ActiveIcon
        {
            get => _activeIcon;
            set
            {
                _activeIcon = value;
                NotifyOfPropertyChange();
            }
        }

        public string ToggleText
        {
            get => _toggleText;
            set
            {
                if (value == _toggleText)
                    return;
                _toggleText = value;
                NotifyOfPropertyChange(() => ToggleText);
            }
        }

        protected override void OnViewReady(object view)
        {
            base.OnViewReady(view);

            Task.Run(() => StartupHide());
        }

        private void StartupHide()
        {
            // TODO: This is probably an awful idea. I can't reliably hook into the view being ready to be hidden
            Thread.Sleep(500);

            if (GeneralSettings.ShowOnStartup || Environment.GetCommandLineArgs().Contains("--show"))
                ShowWindow();
            else
                HideWindow();

            if (!GeneralSettings.Suspended)
                MainManager.EnableProgram();
        }

        public override void CanClose(Action<bool> callback)
        {
            if (CanHideWindow)
                HideWindow();
            else if (!_exiting)
                ShowWindow();

            // Only close if ExitApplication was called
            callback(_exiting);
        }

        public void ShowWindow()
        {
            if (CanShowWindow)
                Window?.Dispatcher.Invoke(() =>
                {
                    Window.Show();
                    Window.Activate();
                });

            GeneralSettings.ApplyTheme();

            // Show certain dialogs if needed
            CheckKeyboardState();
            CheckDuplicateInstances();

            // Run this on the UI thread to avoid having to use dispatchers in VMs
            Execute.OnUIThread(ActivateViews);
        }

        private void ActivateViews()
        {
            var vms = _kernel.GetAll<BaseViewModel>()
                .OrderBy(v => v.DisplayName != "Welcome")
                .ThenBy(v => v.DisplayName)
                .ToList();

            Items.Clear();
            Items.AddRange(vms);
            ActivateItem(vms.FirstOrDefault());

            NotifyOfPropertyChange(() => CanShowWindow);
            NotifyOfPropertyChange(() => CanHideWindow);
        }

        public void HideWindow()
        {
            if (CanHideWindow)
                Window?.Dispatcher.Invoke(() => { Window.Hide(); });

            Items.Clear();
            NotifyOfPropertyChange(() => CanShowWindow);
            NotifyOfPropertyChange(() => CanHideWindow);

            // Force a GC since the UI releases a lot of resources
            GC.Collect();
        }

        public void ToggleEnabled()
        {
            if (Enabled)
                MainManager.DisableProgram();
            else
                MainManager.EnableProgram();
        }

        public void ExitApplication()
        {
            MainManager.Dispose();

            try
            {
                var icon = (TaskbarIcon) Window.FindResource("SystemTrayIcon");
                icon.Dispose();
            }
            catch (Exception)
            {
                //ignored
            }

            _exiting = true;

            // TODO: CoolerMaster SDK is freezing Artemis on shutdown, dunno what to do about it yet
            System.Diagnostics.Process.GetCurrentProcess().Kill();
            // Application.Current.Shutdown();
        }

        public void Settings()
        {
            Flyouts.First().IsOpen = !Flyouts.First().IsOpen;
        }

        public void CloseSettings()
        {
            Flyouts.First().IsOpen = false;
        }

        private async void CheckKeyboardState()
        {
            var dialog = await MetroDialogService.ShowProgressDialog("Enabling keyboard",
                "Artemis is still busy trying to enable your last used keyboard. " +
                "Please wait while the process completes");
            dialog.SetIndeterminate();

            while (MainManager.DeviceManager.ChangingKeyboard)
                await Task.Delay(1000);

            try
            {
                await dialog.CloseAsync();
            }
            catch (InvalidOperationException)
            {
                // Occurs when window is closed again, can't find a proper check for this
            }
        }

        private void CheckDuplicateInstances()
        {
            if (_checkedForDuplicate)
                return;
            _checkedForDuplicate = true;

            Mutex = new Mutex(true, "ArtemisMutex", out var aIsNewInstance);
            if (aIsNewInstance)
                return;

            MetroDialogService.ShowMessageBox("Multiple instances found",
                "It looks like there are multiple running instances of Artemis. " +
                "This can cause issues, especially with CS:GO and Dota2. " +
                "If so, please make sure Artemis isn't already running");
        }
    }
}