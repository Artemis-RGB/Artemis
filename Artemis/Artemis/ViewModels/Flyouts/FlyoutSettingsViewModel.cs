using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Artemis.Models;
using Caliburn.Micro;
using MahApps.Metro.Controls;

namespace Artemis.ViewModels.Flyouts
{
    public class FlyoutSettingsViewModel : FlyoutBaseViewModel
    {
        private string _selectedKeyboardProvider;
        public MainModel MainModel { get; set; }

        public FlyoutSettingsViewModel(MainModel mainModel)
        {
            MainModel = mainModel;
            Header = "settings";
            Position = Position.Right;
        }

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

                MainModel.ChangeKeyboard(MainModel.KeyboardProviders.First(k => k.Name == _selectedKeyboardProvider));
            }
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