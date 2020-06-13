using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services;
using Artemis.UI.Events;
using Artemis.UI.Screens.Settings;
using Artemis.UI.Screens.Sidebar;
using Artemis.UI.Utilities;
using MaterialDesignThemes.Wpf;
using Stylet;

namespace Artemis.UI.Screens
{
    public class RootViewModel : Conductor<IScreen>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly PluginSetting<ApplicationColorScheme> _colorScheme;
        private bool _lostFocus;
        private readonly ThemeWatcher _themeWatcher;

        public RootViewModel(IEventAggregator eventAggregator, SidebarViewModel sidebarViewModel, ISettingsService settingsService)
        {
            SidebarViewModel = sidebarViewModel;
            _eventAggregator = eventAggregator;

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
        public bool IsSidebarVisible { get; set; }
        public bool ActiveItemReady { get; set; }

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

        public void WindowKeyDown(object sender, KeyEventArgs e)
        {
            _eventAggregator.Publish(new MainWindowKeyEvent(true, e));
        }

        public void WindowKeyUp(object sender, KeyEventArgs e)
        {
            _eventAggregator.Publish(new MainWindowKeyEvent(false, e));
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