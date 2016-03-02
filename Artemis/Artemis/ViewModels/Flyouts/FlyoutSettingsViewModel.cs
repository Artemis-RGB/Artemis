using System.Diagnostics;
using System.Linq;
using Artemis.Events;
using Artemis.Managers;
using Artemis.Settings;
using Caliburn.Micro;
using MahApps.Metro.Controls;

namespace Artemis.ViewModels.Flyouts
{
    public class FlyoutSettingsViewModel : FlyoutBaseViewModel, IHandle<ToggleEnabled>, IHandle<ActiveEffectChanged>
    {
        private string _activeEffectName;
        private bool _enabled;
        private GeneralSettings _generalSettings;
        private string _selectedKeyboardProvider;

        public FlyoutSettingsViewModel(MainManager mainManager)
        {
            MainManager = mainManager;
            Header = "Settings";
            Position = Position.Right;
            GeneralSettings = new GeneralSettings();

            MainManager.Events.Subscribe(this);
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

        public MainManager MainManager { get; set; }

        public BindableCollection<string> KeyboardProviders
        {
            get
            {
                var collection = new BindableCollection<string>(MainManager.KeyboardManager.KeyboardProviders
                    .Select(k => k.Name));
                collection.Insert(0, "None");
                return collection;
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
                if (value == null)
                    return;

                MainManager.KeyboardManager.ChangeKeyboard(
                    MainManager.KeyboardManager.KeyboardProviders.FirstOrDefault(
                        k => k.Name == _selectedKeyboardProvider));
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
            var effectDisplay = message.ActiveEffect.Length > 0 ? message.ActiveEffect : "none";
            ActiveEffectName = $"Active effect: {effectDisplay}";
        }

        public void Handle(ToggleEnabled message)
        {
            NotifyOfPropertyChange(() => Enabled);
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
            SelectedKeyboardProvider = General.Default.LastKeyboard.Length > 0
                ? General.Default.LastKeyboard
                : "None";
        }
    }
}