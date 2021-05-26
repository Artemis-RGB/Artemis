using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
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
    public sealed class RootViewModel : Screen, IDisposable
    {
        private readonly IRegistrationService _builtInRegistrationService;
        private readonly ICoreService _coreService;
        private readonly IDebugService _debugService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Timer _frameTimeUpdateTimer;
        private readonly IKernel _kernel;
        private readonly IMessageService _messageService;
        private readonly ISettingsService _settingsService;
        private readonly IWindowManager _windowManager;
        private readonly PluginSetting<WindowSize> _windowSize;
        private string _frameTime;
        private bool _lostFocus;
        private ISnackbarMessageQueue _mainMessageQueue;
        private MaterialWindow _window;
        private string _windowTitle;

        public RootViewModel(
            IKernel kernel,
            IEventAggregator eventAggregator,
            ISettingsService settingsService,
            ICoreService coreService,
            IWindowManager windowManager,
            IDebugService debugService,
            IRegistrationService builtInRegistrationService,
            IMessageService messageService,
            SidebarViewModel sidebarViewModel)
        {
            _kernel = kernel;
            _eventAggregator = eventAggregator;
            _settingsService = settingsService;
            _coreService = coreService;
            _windowManager = windowManager;
            _debugService = debugService;
            _builtInRegistrationService = builtInRegistrationService;
            _messageService = messageService;
            _frameTimeUpdateTimer = new Timer(500);
            _windowSize = _settingsService.GetSetting<WindowSize>("UI.RootWindowSize");

            SidebarViewModel = sidebarViewModel;
            SidebarViewModel.ConductWith(this);


            AssemblyInformationalVersionAttribute versionAttribute = typeof(RootViewModel).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            WindowTitle = $"Artemis {versionAttribute?.InformationalVersion} build {Constants.BuildInfo.BuildNumberDisplay}";
        }

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

        public string FrameTime
        {
            get => _frameTime;
            set => SetAndNotify(ref _frameTime, value);
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

        public void ShowDebugger()
        {
            _debugService.ShowDebugger();
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

        private void UpdateFrameTime()
        {
            FrameTime = $"Frame time: {_coreService.FrameTime.TotalMilliseconds:F2} ms";
        }

        private void OnFrameTimeUpdateTimerOnElapsed(object sender, ElapsedEventArgs args)
        {
            UpdateFrameTime();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _frameTimeUpdateTimer?.Dispose();
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
            UpdateFrameTime();

            _builtInRegistrationService.RegisterBuiltInDataModelDisplays();
            _builtInRegistrationService.RegisterBuiltInDataModelInputs();
            _builtInRegistrationService.RegisterBuiltInPropertyEditors();

            _frameTimeUpdateTimer.Elapsed += OnFrameTimeUpdateTimerOnElapsed;
            _frameTimeUpdateTimer.Start();

            _window = (MaterialWindow) View;

            PluginSetting<bool> setupWizardCompleted = _settingsService.GetSetting("UI.SetupWizardCompleted", false);
            if (!setupWizardCompleted.Value)
                ShowSetupWizard();

            base.OnInitialActivate();
        }

        private void ShowSetupWizard()
        {
            _windowManager.ShowDialog(_kernel.Get<StartupWizardViewModel>());
        }

        protected override void OnClose()
        {
            // Ensure no element with focus can leak, if we don't do this the root VM is retained by Window.EffectiveValues
            // https://stackoverflow.com/a/30864434
            Keyboard.ClearFocus();

            MainMessageQueue = null;
            _frameTimeUpdateTimer.Stop();

            _windowSize.Value ??= new WindowSize();
            _windowSize.Value.ApplyFromWindow(_window);
            _windowSize.Save();

            _frameTimeUpdateTimer.Elapsed -= OnFrameTimeUpdateTimerOnElapsed;

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