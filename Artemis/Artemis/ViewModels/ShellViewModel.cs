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
        private readonly WelcomeViewModel _welcomeVm;

        public ShellViewModel()
        {
            IEventAggregator events = new EventAggregator();
            MainModel = new MainModel(events);
            DisplayName = "Artemis";

            _welcomeVm = new WelcomeViewModel {DisplayName = "Welcome"};
            _effectsVm = new EffectsViewModel(MainModel) {DisplayName = "Effects"};
            _gamesVm = new GamesViewModel(MainModel) {DisplayName = "Games"};
            _overlaysVm = new OverlaysViewModel(MainModel) {DisplayName = "Overlays"};

            Flyouts.Add(new FlyoutSettingsViewModel(MainModel));
        }

        public IObservableCollection<FlyoutBaseViewModel> Flyouts { get; set; } =
            new BindableCollection<FlyoutBaseViewModel>();

        public MainModel MainModel { get; set; }

        protected override void OnActivate()
        {
            base.OnActivate();

            ActivateItem(_welcomeVm);
            ActivateItem(_effectsVm);
            ActivateItem(_gamesVm);
            ActivateItem(_overlaysVm);

            ActiveItem = _welcomeVm;
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