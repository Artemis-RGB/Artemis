using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using Artemis.DAL;
using Artemis.Events;
using Artemis.Managers;
using Artemis.Settings;
using Artemis.Utilities;
using Caliburn.Micro;
using MahApps.Metro.Controls;
using NLog;
using ILogger = Ninject.Extensions.Logging.ILogger;

namespace Artemis.ViewModels.Flyouts
{
    public sealed class FlyoutSettingsViewModel : FlyoutBaseViewModel
    {
        private readonly DebugViewModel _debugViewModel;
        private readonly ILogger _logger;
        private string _activeEffectName;
        private bool _enableDebug;
        private GeneralSettings _generalSettings;
        private string _selectedKeyboardProvider;

        public FlyoutSettingsViewModel(MainManager mainManager, ILogger logger, DebugViewModel debugViewModel)
        {
            _logger = logger;
            _debugViewModel = debugViewModel;

            MainManager = mainManager;
            Header = "Settings";
            Position = Position.Right;
            GeneralSettings = SettingsProvider.Load<GeneralSettings>();

            LogLevels = new BindableCollection<string>();
            LogLevels.AddRange(LogLevel.AllLoggingLevels.Select(l => l.Name));

            PropertyChanged += KeyboardUpdater;
            mainManager.OnEnabledChangedEvent += MainManagerOnOnEnabledChangedEvent;
            mainManager.EffectManager.OnEffectChangedEvent += EffectManagerOnOnEffectChangedEvent;
        }

        public MainManager MainManager { get; set; }

        public bool EnableDebug
        {
            get { return _enableDebug; }
            set
            {
                if (value == _enableDebug) return;
                _enableDebug = value;
                NotifyOfPropertyChange(() => EnableDebug);
            }
        }

        public GeneralSettings GeneralSettings
        {
            get { return _generalSettings; }
            set
            {
                if (Equals(value, _generalSettings)) return;
                _generalSettings = value;
                NotifyOfPropertyChange(() => GeneralSettings);
            }
        }

        public BindableCollection<string> KeyboardProviders
        {
            get
            {
                return new BindableCollection<string>(MainManager.DeviceManager.KeyboardProviders.Select(k => k.Name));
            }
        }

        public BindableCollection<string> Themes => new BindableCollection<string>
        {
            "Light",
            "Dark",
            "Corsair Light",
            "Corsair Dark"
        };

        public BindableCollection<string> Layouts => new BindableCollection<string>
        {
            "Qwerty",
            "Qwertz",
            "Azerty"
        };

        public string VersionText => "Artemis " + Assembly.GetExecutingAssembly().GetName().Version;

        public BindableCollection<string> LogLevels { get; set; }

        public string SelectedTheme
        {
            get { return GeneralSettings.Theme; }
            set
            {
                if (value == GeneralSettings.Theme) return;
                GeneralSettings.Theme = value;
                NotifyOfPropertyChange(() => SelectedTheme);
            }
        }

        public string SelectedLayout
        {
            get { return GeneralSettings.Layout; }
            set
            {
                if (value == GeneralSettings.Layout) return;
                GeneralSettings.Layout = value;
                NotifyOfPropertyChange(() => SelectedLayout);
            }
        }

        public string SelectedLogLevel
        {
            get { return GeneralSettings.LogLevel; }
            set
            {
                if (value == GeneralSettings.LogLevel) return;
                GeneralSettings.LogLevel = value;
                NotifyOfPropertyChange(() => SelectedLogLevel);
            }
        }

        public string SelectedKeyboardProvider
        {
            get { return _selectedKeyboardProvider; }
            set
            {
                if (value == _selectedKeyboardProvider) return;
                _selectedKeyboardProvider = value;
                NotifyOfPropertyChange(() => SelectedKeyboardProvider);
            }
        }

        public bool Enabled
        {
            get { return MainManager.ProgramEnabled; }
            set
            {
                if (value)
                    MainManager.EnableProgram();
                else
                    MainManager.DisableProgram();
            }
        }

        public string ActiveEffectName
        {
            get { return _activeEffectName; }
            set
            {
                if (value == _activeEffectName) return;
                _activeEffectName = value;
                NotifyOfPropertyChange(() => ActiveEffectName);
            }
        }

        private void MainManagerOnOnEnabledChangedEvent(object sender, EnabledChangedEventArgs enabledChangedEventArgs)
        {
            NotifyOfPropertyChange(() => Enabled);
        }

        private void EffectManagerOnOnEffectChangedEvent(object sender, EffectChangedEventArgs e)
        {
            var effectDisplay = string.IsNullOrEmpty(e.Effect?.Name) ? "none" : e.Effect.Name;
            ActiveEffectName = $"Active effect: {effectDisplay}";
        }

        /// <summary>
        ///     Takes proper action when the selected keyboard is changed in the UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KeyboardUpdater(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "SelectedKeyboardProvider")
                return;

            _logger.Debug("Handling SelectedKeyboard change in UI");
            var keyboard = MainManager.DeviceManager.KeyboardProviders
                .FirstOrDefault(k => k.Name == SelectedKeyboardProvider);
            if (keyboard != null)
            {
                MainManager.DeviceManager.EnableKeyboard(keyboard);
                MainManager.LoopManager.StartAsync();
            }
            else
                MainManager.DeviceManager.ReleaseActiveKeyboard(true);
        }

        public void ToggleEnabled()
        {
            if (Enabled)
                MainManager.DisableProgram();
            else
                MainManager.EnableProgram();
        }

        public void ShowDebug()
        {
            IWindowManager manager = new WindowManager();
            dynamic settings = new ExpandoObject();
            var icon = ImageUtilities.GenerateWindowIcon();

            settings.Title = "Artemis | Debugger";
            settings.Icon = icon;

            manager.ShowWindow(_debugViewModel, null, settings);
        }

        public void ResetSettings()
        {
            GeneralSettings.Reset(true);
            NotifyOfPropertyChange(() => GeneralSettings);
        }

        public void SaveSettings()
        {
            GeneralSettings.Save();
        }

        public void NavigateTo(string url)
        {
            System.Diagnostics.Process.Start(new ProcessStartInfo(url));
        }

        protected override void HandleOpen()
        {
            SelectedKeyboardProvider = string.IsNullOrEmpty(GeneralSettings.LastKeyboard)
                ? "None"
                : GeneralSettings.LastKeyboard;
        }
    }
}