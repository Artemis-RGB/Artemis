using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Events;
using Artemis.UI.Screens.Sidebar;
using Artemis.UI.Screens.StartupWizard;
using Artemis.UI.Services;
using Artemis.UI.Shared.Services;
using Artemis.UI.Utilities;
using MaterialDesignExtensions.Controls;
using MaterialDesignThemes.Wpf;
using Ninject;
using Stylet;
using Constants = Artemis.Core.Constants;

namespace Artemis.UI.Screens
{
    public sealed class RootViewModel : Conductor<Screen>
    {
        private readonly IRegistrationService _builtInRegistrationService;

        private readonly IEventAggregator _eventAggregator;
        private readonly IKernel _kernel;
        private readonly IMessageService _messageService;
        private readonly ISettingsService _settingsService;
        private readonly IWindowManager _windowManager;
        private readonly PluginSetting<WindowSize> _windowSize;

        private bool _lostFocus;
        private ISnackbarMessageQueue _mainMessageQueue;
        private MaterialWindow _window;
        private string _windowTitle;

        public RootViewModel(
            IKernel kernel,
            IEventAggregator eventAggregator,
            ISettingsService settingsService,
            IWindowManager windowManager,
            IRegistrationService builtInRegistrationService,
            IMessageService messageService,
            SidebarViewModel sidebarViewModel)
        {
            _kernel = kernel;
            _eventAggregator = eventAggregator;
            _settingsService = settingsService;
            _windowManager = windowManager;
            _builtInRegistrationService = builtInRegistrationService;
            _messageService = messageService;
            _windowSize = _settingsService.GetSetting<WindowSize>("UI.RootWindowSize");

            SidebarWidth = _settingsService.GetSetting("UI.SidebarWidth", new GridLength(240));
            SidebarViewModel = sidebarViewModel;
            SidebarViewModel.ConductWith(this);

            AssemblyInformationalVersionAttribute versionAttribute = typeof(RootViewModel).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            WindowTitle = $"Artemis {versionAttribute?.InformationalVersion} build {Constants.BuildInfo.BuildNumberDisplay}";
        }

        public PluginSetting<GridLength> SidebarWidth { get; }
        public SidebarViewModel SidebarViewModel { get; }

        public ISnackbarMessageQueue MainMessageQueue
        {
            get => _mainMessageQueue;
            set => SetAndNotify(ref _mainMessageQueue, value);
        }

        public string WindowTitle
        {
            get => _windowTitle;
            set => SetAndNotify(ref _windowTitle, value);
        }


        public void WindowDeactivated()
        {
            WindowState windowState = ((Window) View).WindowState;
            if (windowState == WindowState.Minimized)
                return;

            _lostFocus = true;
            _eventAggregator.Publish(new MainWindowFocusChangedEvent(false));
        }

        public void WindowActivated()
        {
            if (!_lostFocus)
                return;

            _lostFocus = false;
            _eventAggregator.Publish(new MainWindowFocusChangedEvent(true));
        }

        public void WindowKeyDown(object sender, KeyEventArgs e)
        {
            _eventAggregator.Publish(new MainWindowKeyEvent(sender, true, e));
        }

        public void WindowKeyUp(object sender, KeyEventArgs e)
        {
            _eventAggregator.Publish(new MainWindowKeyEvent(sender, false, e));
        }

        public void WindowMouseDown(object sender, MouseButtonEventArgs e)
        {
            _eventAggregator.Publish(new MainWindowMouseEvent(sender, true, e));
        }

        public void WindowMouseUp(object sender, MouseButtonEventArgs e)
        {
            _eventAggregator.Publish(new MainWindowMouseEvent(sender, false, e));
        }

        private void SidebarViewModelOnSelectedScreenChanged(object? sender, EventArgs e)
        {
            ActiveItem = SidebarViewModel.SelectedScreen;
        }

        private void ShowSetupWizard()
        {
            _windowManager.ShowDialog(_kernel.Get<StartupWizardViewModel>());
        }

        #region Overrides of Screen

        protected override void OnViewLoaded()
        {
            MaterialWindow window = (MaterialWindow) View;
            if (_windowSize.Value != null)
            {
                _windowSize.Value.ApplyToWindow(window);
            }
            else
            {
                _windowSize.Value = new WindowSize();
                _windowSize.Value.ApplyFromWindow(window);
            }

            base.OnViewLoaded();
        }

        protected override void OnInitialActivate()
        {
            MainMessageQueue = _messageService.MainMessageQueue;
            SidebarViewModel.SelectedScreenChanged += SidebarViewModelOnSelectedScreenChanged;
            ActiveItem = SidebarViewModel.SelectedScreen;

            _builtInRegistrationService.RegisterBuiltInDataModelDisplays();
            _builtInRegistrationService.RegisterBuiltInDataModelInputs();
            _builtInRegistrationService.RegisterBuiltInPropertyEditors();

            _window = (MaterialWindow) View;

            PluginSetting<bool> setupWizardCompleted = _settingsService.GetSetting("UI.SetupWizardCompleted", false);
            if (!setupWizardCompleted.Value)
                ShowSetupWizard();

            base.OnInitialActivate();
        }

        protected override void OnClose()
        {
            // Ensure no element with focus can leak, if we don't do this the root VM is retained by Window.EffectiveValues
            // https://stackoverflow.com/a/30864434
            Keyboard.ClearFocus();

            MainMessageQueue = null;

            SidebarViewModel.SelectedScreenChanged -= SidebarViewModelOnSelectedScreenChanged;
            SidebarWidth.Save();
            _windowSize.Value ??= new WindowSize();
            _windowSize.Value.ApplyFromWindow(_window);
            _windowSize.Save();

            // Lets force the GC to run after closing the window so it is obvious to users watching task manager
            // that closing the UI will decrease the memory footprint of the application.
            Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(15));
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            });

            base.OnClose();
        }

        #endregion
    }
}