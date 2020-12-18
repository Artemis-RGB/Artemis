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
using Artemis.UI.Screens.Modules;
using Artemis.UI.Screens.Settings.Tabs.General;
using Artemis.UI.Screens.Sidebar;
using Artemis.UI.Screens.StartupWizard;
using Artemis.UI.Services;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Utilities;
using MaterialDesignExtensions.Controls;
using MaterialDesignThemes.Wpf;
using Ninject;
using Stylet;

namespace Artemis.UI.Screens
{
    public sealed class RootViewModel : Conductor<IScreen>, IDisposable
    {
        private readonly IRegistrationService _builtInRegistrationService;
        private readonly PluginSetting<ApplicationColorScheme> _colorScheme;
        private readonly ICoreService _coreService;
        private readonly IWindowManager _windowManager;
        private readonly IDebugService _debugService;
        private readonly IKernel _kernel;
        private readonly IEventAggregator _eventAggregator;
        private readonly ISettingsService _settingsService;
        private readonly Timer _frameTimeUpdateTimer;
        private readonly SidebarViewModel _sidebarViewModel;
        private readonly ISnackbarMessageQueue _snackbarMessageQueue;
        private readonly ThemeWatcher _themeWatcher;
        private readonly PluginSetting<WindowSize> _windowSize;
        private bool _activeItemReady;
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
            ISnackbarMessageQueue snackbarMessageQueue,
            SidebarViewModel sidebarViewModel)
        {
            _kernel = kernel;
            _eventAggregator = eventAggregator;
            _settingsService = settingsService;
            _coreService = coreService;
            _windowManager = windowManager;
            _debugService = debugService;
            _builtInRegistrationService = builtInRegistrationService;
            _snackbarMessageQueue = snackbarMessageQueue;
            _sidebarViewModel = sidebarViewModel;

            _frameTimeUpdateTimer = new Timer(500);

            _colorScheme = _settingsService.GetSetting("UI.ColorScheme", ApplicationColorScheme.Automatic);
            _windowSize = _settingsService.GetSetting<WindowSize>("UI.RootWindowSize");

            _themeWatcher = new ThemeWatcher();
            ApplyColorSchemeSetting();

            ActiveItem = sidebarViewModel.SelectedItem;
            ActiveItemReady = true;
            PinSidebar = _settingsService.GetSetting("UI.PinSidebar", false);

            AssemblyInformationalVersionAttribute versionAttribute = typeof(RootViewModel).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            WindowTitle = $"Artemis {versionAttribute?.InformationalVersion}";
        }

        public PluginSetting<bool> PinSidebar { get; }

        // Just a litte trick to get the non-active variant completely removed from XAML (that should probably be done in the view)
        public SidebarViewModel PinnedSidebarViewModel => PinSidebar.Value ? _sidebarViewModel : null;
        public SidebarViewModel DockedSidebarViewModel => PinSidebar.Value ? null : _sidebarViewModel;

        public ISnackbarMessageQueue MainMessageQueue
        {
            get => _mainMessageQueue;
            set => SetAndNotify(ref _mainMessageQueue, value);
        }

        public bool ActiveItemReady
        {
            get => _activeItemReady;
            set => SetAndNotify(ref _activeItemReady, value);
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

        private void UpdateSidebarPinState()
        {
            _sidebarViewModel.IsSidebarOpen = true;

            NotifyOfPropertyChange(nameof(PinnedSidebarViewModel));
            NotifyOfPropertyChange(nameof(DockedSidebarViewModel));
        }

        private void UpdateFrameTime()
        {
            FrameTime = $"Frame time: {_coreService.FrameTime.TotalMilliseconds:F2} ms";
        }

        private void SidebarViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_sidebarViewModel.SelectedItem) && ActiveItem != _sidebarViewModel.SelectedItem)
            {
                // Unless the sidebar is pinned, close it upon selecting an item
                if (!PinSidebar.Value)
                    _sidebarViewModel.IsSidebarOpen = false;

                // Don't do a fade when selecting a module because the editor is so bulky the animation slows things down
                if (!(_sidebarViewModel.SelectedItem is ModuleRootViewModel))
                    ActiveItemReady = false;

                // Allow the menu to close, it's slower but feels more responsive, funny how that works right
                Execute.PostToUIThreadAsync(async () =>
                {
                    if (PinSidebar.Value)
                        await Task.Delay(200);
                    else
                        await Task.Delay(400);

                    ActiveItem = _sidebarViewModel.SelectedItem;
                    ActiveItemReady = true;
                });
            }
        }

        private void ApplyColorSchemeSetting()
        {
            if (_colorScheme.Value == ApplicationColorScheme.Automatic)
                ApplyWindowsTheme(_themeWatcher.GetWindowsTheme());
            else
                ChangeMaterialColors(_colorScheme.Value);
        }

        private void ApplyWindowsTheme(ThemeWatcher.WindowsTheme windowsTheme)
        {
            if (_colorScheme.Value != ApplicationColorScheme.Automatic)
                return;

            if (windowsTheme == ThemeWatcher.WindowsTheme.Dark)
                ChangeMaterialColors(ApplicationColorScheme.Dark);
            else
                ChangeMaterialColors(ApplicationColorScheme.Light);
        }

        private void ChangeMaterialColors(ApplicationColorScheme colorScheme)
        {
            PaletteHelper paletteHelper = new();
            ITheme theme = paletteHelper.GetTheme();
            theme.SetBaseTheme(colorScheme == ApplicationColorScheme.Dark ? Theme.Dark : Theme.Light);
            paletteHelper.SetTheme(theme);

            MaterialDesignExtensions.Themes.PaletteHelper extensionsPaletteHelper = new();
            extensionsPaletteHelper.SetLightDark(colorScheme == ApplicationColorScheme.Dark);
        }

        private void OnFrameTimeUpdateTimerOnElapsed(object sender, ElapsedEventArgs args)
        {
            UpdateFrameTime();
        }

        private void ThemeWatcherOnThemeChanged(object sender, WindowsThemeEventArgs e)
        {
            ApplyWindowsTheme(e.Theme);
        }

        private void ColorSchemeOnSettingChanged(object sender, EventArgs e)
        {
            ApplyColorSchemeSetting();
        }

        private void PinSidebarOnSettingChanged(object sender, EventArgs e)
        {
            UpdateSidebarPinState();
        }

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            _frameTimeUpdateTimer?.Dispose();
        }

        #endregion

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
            MainMessageQueue = _snackbarMessageQueue;
            UpdateFrameTime();

            _builtInRegistrationService.RegisterBuiltInDataModelDisplays();
            _builtInRegistrationService.RegisterBuiltInDataModelInputs();
            _builtInRegistrationService.RegisterBuiltInPropertyEditors();

            _frameTimeUpdateTimer.Elapsed += OnFrameTimeUpdateTimerOnElapsed;
            _colorScheme.SettingChanged += ColorSchemeOnSettingChanged;
            _themeWatcher.ThemeChanged += ThemeWatcherOnThemeChanged;
            _sidebarViewModel.PropertyChanged += SidebarViewModelOnPropertyChanged;
            PinSidebar.SettingChanged += PinSidebarOnSettingChanged;

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
            _colorScheme.SettingChanged -= ColorSchemeOnSettingChanged;
            _themeWatcher.ThemeChanged -= ThemeWatcherOnThemeChanged;
            _sidebarViewModel.PropertyChanged -= SidebarViewModelOnPropertyChanged;
            PinSidebar.SettingChanged -= PinSidebarOnSettingChanged;

            _sidebarViewModel.Dispose();

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