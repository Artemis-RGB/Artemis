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
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Utilities;
using MaterialDesignThemes.Wpf;
using Stylet;

namespace Artemis.UI.Screens
{
    public class RootViewModel : Conductor<IScreen>
    {
        private readonly PluginSetting<ApplicationColorScheme> _colorScheme;
        private readonly ICoreService _coreService;
        private readonly IDebugService _debugService;
        private readonly IEventAggregator _eventAggregator;
        private readonly ThemeWatcher _themeWatcher;
        private readonly Timer _titleUpdateTimer;
        private bool _lostFocus;

        public RootViewModel(IEventAggregator eventAggregator, SidebarViewModel sidebarViewModel, ISettingsService settingsService, ICoreService coreService,
            IDebugService debugService, ISnackbarMessageQueue snackbarMessageQueue)
        {
            SidebarViewModel = sidebarViewModel;
            MainMessageQueue = snackbarMessageQueue;
            _eventAggregator = eventAggregator;
            _coreService = coreService;
            _debugService = debugService;

            _titleUpdateTimer = new Timer(500);
            _titleUpdateTimer.Elapsed += (sender, args) => UpdateWindowTitle();
            _colorScheme = settingsService.GetSetting("UI.ColorScheme", ApplicationColorScheme.Automatic);
            _colorScheme.SettingChanged += (sender, args) => ApplyColorSchemeSetting();
            _themeWatcher = new ThemeWatcher();
            _themeWatcher.ThemeChanged += (sender, args) => ApplyWindowsTheme(args.Theme);
            ApplyColorSchemeSetting();

            ActiveItem = SidebarViewModel.SelectedItem;
            ActiveItemReady = true;
            SidebarViewModel.PropertyChanged += SidebarViewModelOnPropertyChanged;
        }

        public SidebarViewModel SidebarViewModel { get; }
        public ISnackbarMessageQueue MainMessageQueue { get; set; }
        public bool IsSidebarVisible { get; set; }
        public bool ActiveItemReady { get; set; }
        public string WindowTitle { get; set; }
        
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
            _eventAggregator.Publish(new MainWindowKeyEvent(true, e));
        }

        public void WindowKeyUp(object sender, KeyEventArgs e)
        {
            _eventAggregator.Publish(new MainWindowKeyEvent(false, e));
        }

        protected override void OnActivate()
        {
            UpdateWindowTitle();
            _titleUpdateTimer.Start();
        }

        protected override void OnDeactivate()
        {
            _titleUpdateTimer.Stop();
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
    }
}