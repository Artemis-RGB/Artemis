using System;
using System.Linq;
using System.Windows;
using Artemis.Models;
using Artemis.ViewModels.Flyouts;
using Caliburn.Micro;

namespace Artemis.ViewModels
{
    public sealed class ShellViewModel : Conductor<IScreen>.Collection.OneActive
    {
        private readonly EffectsViewModel _effectsVm;
        private readonly GamesViewModel _gamesVm;
        private readonly OverlaysViewModel _overlaysVm;

        public ShellViewModel()
        {
            IEventAggregator events = new EventAggregator();
            MainModel = new MainModel(events);
            DisplayName = "Artemis";

            _effectsVm = new EffectsViewModel(MainModel) {DisplayName = "Effects"};
            _gamesVm = new GamesViewModel(MainModel) { DisplayName = "Games" };
            _overlaysVm = new OverlaysViewModel(MainModel) {DisplayName = "Overlays"};
            
            Flyouts.Add(new FlyoutSettingsViewModel());

            // By now Effects are added to the MainModel so we can savely start one
            ToggleEffects();
        }

        public IObservableCollection<FlyoutBaseViewModel> Flyouts { get; set; } =
            new BindableCollection<FlyoutBaseViewModel>();

        protected override void OnActivate()
        {
            base.OnActivate();

            ActivateItem(_effectsVm);
            ActivateItem(_gamesVm);
            ActivateItem(_overlaysVm);
        }

        public bool EffectsEnabled
        {
            get { return MainModel.Enabled; }
            private set
            {
                MainModel.Enabled = value;
                NotifyOfPropertyChange(() => EffectsEnabled);
            }
        }

        public MainModel MainModel { get; set; }

        public void ToggleEffects()
        {
            if (EffectsEnabled)
                MainModel.ShutdownEffects();
            else
                MainModel.StartEffects();

            EffectsEnabled = !EffectsEnabled;
        }

        public void OnClose(EventArgs e)
        {
            MainModel.ShutdownEffects();
            Application.Current.Shutdown();
        }

        public void Settings()
        {
            Flyouts.First().IsOpen = !Flyouts.First().IsOpen;
        }
    }
}