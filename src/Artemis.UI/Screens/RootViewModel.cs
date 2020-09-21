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
using Artemis.UI.Services;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Utilities;
using MaterialDesignExtensions.Controls;
using MaterialDesignThemes.Wpf;
using Stylet;

namespace Artemis.UI.Screens
{
    public class RootViewModel : Conductor<IScreen>
    {
        private readonly IRegistrationService _builtInRegistrationService;
        private readonly PluginSetting<ApplicationColorScheme> _colorScheme;
        private readonly ICoreService _coreService;
        private readonly IDebugService _debugService;
        private readonly IEventAggregator _eventAggregator;
        private readonly ISnackbarMessageQueue _snackbarMessageQueue;
        private readonly ThemeWatcher _themeWatcher;
        private readonly Timer _frameTimeUpdateTimer;
        private readonly PluginSetting<WindowSize> _windowSize;
        private bool _activeItemReady;
        private bool _lostFocus;
        private ISnackbarMessageQueue _mainMessageQueue;
        private string _windowTitle;
        private string _frameTime;

        public RootViewModel(
            IEventAggregator eventAggregator,
            ISettingsService settingsService,
            ICoreService coreService,
            IDebugService debugService,
            IRegistrationService builtInRegistrationService,
            ISnackbarMessageQueue snackbarMessageQueue,
            SidebarViewModel sidebarViewModel)
        {
            SidebarViewModel = sidebarViewModel;
            _eventAggregator = eventAggregator;
            _coreService = coreService;
            _debugService = debugService;
            _builtInRegistrationService = builtInRegistrationService;
            _snackbarMessageQueue = snackbarMessageQueue;

            _frameTimeUpdateTimer = new Timer(500);

            _colorScheme = settingsService.GetSetting("UI.ColorScheme", ApplicationColorScheme.Automatic);
            _windowSize = settingsService.GetSetting<WindowSize>("UI.RootWindowSize");

            _themeWatcher = new ThemeWatcher();
            ApplyColorSchemeSetting();

            ActiveItem = SidebarViewModel.SelectedItem;
            ActiveItemReady = true;

            var versionAttribute = typeof(RootViewModel).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            WindowTitle = $"Artemis {versionAttribute?.InformationalVersion}";
        }

        public SidebarViewModel SidebarViewModel { get; }

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
            var windowState = ((Window) View).WindowState;
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
        
        private void SidebarViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SidebarViewModel.SelectedItem) && ActiveItem != SidebarViewModel.SelectedItem)
            {
                SidebarViewModel.IsSidebarOpen = false;
                // Don't do a fade when selecting a module because the editor is so bulky the animation slows things down
                if (!(SidebarViewModel.SelectedItem is ModuleRootViewModel)) 
                    ActiveItemReady = false;
                
                // Allow the menu to close, it's slower but feels more responsive, funny how that works right
                Execute.PostToUIThreadAsync(async () =>
                {
                    await Task.Delay(400);
                    ActiveItem = SidebarViewModel.SelectedItem;
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
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();
            theme.SetBaseTheme(colorScheme == ApplicationColorScheme.Dark ? Theme.Dark : Theme.Light);
            paletteHelper.SetTheme(theme);

            var extensionsPaletteHelper = new MaterialDesignExtensions.Themes.PaletteHelper();
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

        #region Overrides of Screen

        protected override void OnViewLoaded()
        {
            var window = (MaterialWindow) View;
            if (_windowSize.Value != null)
                _windowSize.Value.ApplyToWindow(window);
            else
            {
                _windowSize.Value = new WindowSize();
                _windowSize.Value.ApplyFromWindow(window);
            }

            base.OnViewLoaded();
        }

        protected override void OnActivate()
        {
            MainMessageQueue = _snackbarMessageQueue;
            UpdateFrameTime();

            _builtInRegistrationService.RegisterBuiltInDataModelDisplays();
            _builtInRegistrationService.RegisterBuiltInDataModelInputs();
            _builtInRegistrationService.RegisterBuiltInPropertyEditors();

            _frameTimeUpdateTimer.Elapsed += OnFrameTimeUpdateTimerOnElapsed;
            _colorScheme.SettingChanged += ColorSchemeOnSettingChanged;
            _themeWatcher.ThemeChanged += ThemeWatcherOnThemeChanged;
            SidebarViewModel.PropertyChanged += SidebarViewModelOnPropertyChanged;

            _frameTimeUpdateTimer.Start();

            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            // Ensure no element with focus can leak, if we don't do this the root VM is retained by Window.EffectiveValues
            // https://stackoverflow.com/a/30864434
            Keyboard.ClearFocus();

            MainMessageQueue = null;
            _frameTimeUpdateTimer.Stop();

            var window = (MaterialWindow) View;
            _windowSize.Value ??= new WindowSize();
            _windowSize.Value.ApplyFromWindow(window);
            _windowSize.Save();

            _frameTimeUpdateTimer.Elapsed -= OnFrameTimeUpdateTimerOnElapsed;
            _colorScheme.SettingChanged -= ColorSchemeOnSettingChanged;
            _themeWatcher.ThemeChanged -= ThemeWatcherOnThemeChanged;
            SidebarViewModel.PropertyChanged -= SidebarViewModelOnPropertyChanged;

            base.OnDeactivate();
        }

        protected override void OnClose()
        {
            SidebarViewModel.Dispose();
            
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