using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Artemis.Events;
using Artemis.Managers;
using Artemis.Settings;
using Caliburn.Micro;
using MahApps.Metro.Controls;
using NLog;
using NLog.Targets;
using ILogger = Ninject.Extensions.Logging.ILogger;
using LogManager = NLog.LogManager;

namespace Artemis.ViewModels.Flyouts
{
    public sealed class FlyoutSettingsViewModel : FlyoutBaseViewModel, IHandle<ToggleEnabled>,
        IHandle<ActiveEffectChanged>
    {
        private readonly ILogger _logger;
        private string _activeEffectName;
        private bool _enableDebug;
        private GeneralSettings _generalSettings;
        private string _selectedKeyboardProvider;

        public FlyoutSettingsViewModel(MainManager mainManager, IEventAggregator events, ILogger logger)
        {
            _logger = logger;

            MainManager = mainManager;
            Header = "Settings";
            Position = Position.Right;
            GeneralSettings = new GeneralSettings();

            PropertyChanged += KeyboardUpdater;
            events.Subscribe(this);
            ApplyLogging();
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
                var collection =
                    new BindableCollection<string>(MainManager.DeviceManager.KeyboardProviders.Select(k => k.Name));
                collection.Insert(0, "None");
                return collection;
            }
        }

        public BindableCollection<string> Themes
            => new BindableCollection<string> {"Light", "Dark", "Corsair Light", "Corsair Dark"};

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

        public void Handle(ActiveEffectChanged message)
        {
            var effectDisplay = string.IsNullOrEmpty(message.ActiveEffect) ? message.ActiveEffect : "none";
            ActiveEffectName = $"Active effect: {effectDisplay}";
        }

        public void Handle(ToggleEnabled message)
        {
            NotifyOfPropertyChange(() => Enabled);
        }

        // TODO https://github.com/ninject/Ninject.Extensions.Logging/issues/35
        private void ApplyLogging()
        {
            return;
            var c = LogManager.Configuration;
            var file = c.FindTargetByName("file") as FileTarget;
            if (file == null)
                return;

            var rule = c.LoggingRules.FirstOrDefault(r => r.Targets.Contains(file));
            if (rule == null)
                return;

            if (EnableDebug)
                rule.EnableLoggingForLevel(LogLevel.Debug);
            else
                rule.DisableLoggingForLevel(LogLevel.Debug);

            LogManager.ReconfigExistingLoggers();
            _logger.Info("Set debug logging to: {0}", EnableDebug);
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
                MainManager.LoopManager.Start();
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

        public void ResetSettings()
        {
            GeneralSettings.ResetSettings();
            NotifyOfPropertyChange(() => GeneralSettings);
        }

        public void SaveSettings()
        {
            GeneralSettings.SaveSettings();
        }

        public void NavigateTo(string url)
        {
            Process.Start(new ProcessStartInfo(url));
        }

        protected override void HandleOpen()
        {
            SelectedKeyboardProvider = string.IsNullOrEmpty(General.Default.LastKeyboard)
                ? "None"
                : General.Default.LastKeyboard;
        }
    }
}