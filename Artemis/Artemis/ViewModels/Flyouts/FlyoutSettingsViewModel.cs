using System.Diagnostics;
using System.Linq;
using Artemis.Events;
using Artemis.Models;
using Artemis.Settings;
using Caliburn.Micro;
using MahApps.Metro.Controls;

namespace Artemis.ViewModels.Flyouts
{
    public class FlyoutSettingsViewModel : FlyoutBaseViewModel, IHandle<ToggleEnabled>
    {
        private bool _enabled;
        private GeneralSettings _generalSettings;
        private string _selectedKeyboardProvider;

        public FlyoutSettingsViewModel(MainModel mainModel)
        {
            MainModel = mainModel;
            Header = "settings";
            Position = Position.Right;
            GeneralSettings = new GeneralSettings();

            MainModel.Events.Subscribe(this);
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

        public MainModel MainModel { get; set; }

        public BindableCollection<string> KeyboardProviders
            => new BindableCollection<string>(MainModel.KeyboardProviders.Select(k => k.Name));

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

                MainModel.ChangeKeyboard(MainModel.KeyboardProviders.First(k => k.Name == _selectedKeyboardProvider));
            }
        }

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (value == _enabled) return;
                _enabled = value;

                NotifyOfPropertyChange(() => Enabled);
            }
        }

        public void Handle(ToggleEnabled message)
        {
            Enabled = message.Enabled;
        }

        public void ToggleEnabled()
        {
            if (Enabled)
                MainModel.ShutdownEffects();
            else
                MainModel.StartEffects();
        }

        public void NavigateTo(string url)
        {
            Process.Start(new ProcessStartInfo(url));
        }

        protected override void HandleOpen()
        {
            SelectedKeyboardProvider = MainModel.ActiveKeyboard?.Name;
        }
    }
}