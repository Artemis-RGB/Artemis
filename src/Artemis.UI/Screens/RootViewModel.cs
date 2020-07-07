using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services;
using Artemis.Core.Services.Interfaces;
using Artemis.UI.Events;
using Artemis.UI.Screens.Settings;
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
        private readonly PluginSetting<ApplicationColorScheme> _colorScheme;
        private readonly ICoreService _coreService;
        private readonly IDebugService _debugService;
        private readonly IRegistrationService _builtInRegistrationService;
        private readonly IEventAggregator _eventAggregator;
        private readonly ThemeWatcher _themeWatcher;
        private readonly Timer _titleUpdateTimer;
        private readonly PluginSetting<WindowSize> _windowSize;
        private bool _lostFocus;
        private bool _isSidebarVisible;
        private bool _activeItemReady;
        private string _windowTitle;

        public RootViewModel(IEventAggregator eventAggregator, SidebarViewModel sidebarViewModel, ISettingsService settingsService, ICoreService coreService,
            IDebugService debugService, IRegistrationService builtInRegistrationService, ISnackbarMessageQueue snackbarMessageQueue)
        {
            SidebarViewModel = sidebarViewModel;
            MainMessageQueue = snackbarMessageQueue;
            _eventAggregator = eventAggregator;
            _coreService = coreService;
            _debugService = debugService;
            _builtInRegistrationService = builtInRegistrationService;

            _titleUpdateTimer = new Timer(500);
            _titleUpdateTimer.Elapsed += (sender, args) => UpdateWindowTitle();
            _colorScheme = settingsService.GetSetting("UI.ColorScheme", ApplicationColorScheme.Automatic);
            _windowSize = settingsService.GetSetting<WindowSize>("UI.RootWindowSize");
            _colorScheme.SettingChanged += (sender, args) => ApplyColorSchemeSetting();
            _themeWatcher = new ThemeWatcher();
            _themeWatcher.ThemeChanged += (sender, args) => ApplyWindowsTheme(args.Theme);
            ApplyColorSchemeSetting();

            ActiveItem = SidebarViewModel.SelectedItem;
            ActiveItemReady = true;
            SidebarViewModel.PropertyChanged += SidebarViewModelOnPropertyChanged;
        }

        public SidebarViewModel SidebarViewModel { get; }
        public ISnackbarMessageQueue MainMessageQueue { get; }

        public bool IsSidebarVisible
        {
            get => _isSidebarVisible;
            set => SetAndNotify(ref _isSidebarVisible, value);
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


        private void UpdateWindowTitle()
        {
            var versionAttribute = typeof(RootViewModel).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            WindowTitle = $"Artemis {versionAttribute?.InformationalVersion} - Frame time: {_coreService.FrameTime.TotalMilliseconds:F2} ms";
        }

        private void SidebarViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SidebarViewModel.SelectedItem))
            {
                IsSidebarVisible = false;
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
            UpdateWindowTitle();
            _titleUpdateTimer.Start();

            _builtInRegistrationService.RegisterBuiltInDataModelDisplays();
            _builtInRegistrationService.RegisterBuiltInDataModelInputs();
            _builtInRegistrationService.RegisterBuiltInPropertyEditors();
        }

        protected override void OnDeactivate()
        {
            _titleUpdateTimer.Stop();

            var window = (MaterialWindow) View;
            _windowSize.Value ??= new WindowSize();
            _windowSize.Value.ApplyFromWindow(window);
            _windowSize.Save();
        }

        #endregion
    }
}