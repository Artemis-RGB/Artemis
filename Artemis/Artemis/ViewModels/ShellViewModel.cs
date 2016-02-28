using System;
using System.Linq;
using System.Windows;
using Artemis.Managers;
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
            MainManager = new MainManager(events);
            DisplayName = "Artemis";

            _welcomeVm = new WelcomeViewModel {DisplayName = "Welcome"};
            _effectsVm = new EffectsViewModel(MainManager) {DisplayName = "Effects"};
            _gamesVm = new GamesViewModel(MainManager) {DisplayName = "Games"};
            _overlaysVm = new OverlaysViewModel(MainManager) {DisplayName = "Overlays"};

            Flyouts.Add(new FlyoutSettingsViewModel(MainManager));
        }

        public IObservableCollection<FlyoutBaseViewModel> Flyouts { get; set; } =
            new BindableCollection<FlyoutBaseViewModel>();

        public MainManager MainManager { get; set; }

        protected override void OnActivate()
        {
            base.OnActivate();

            ActivateItem(_welcomeVm);
            ActivateItem(_effectsVm);
            ActivateItem(_gamesVm);
            ActivateItem(_overlaysVm);

            ActiveItem = _welcomeVm;
        }

        public void Settings()
        {
            Flyouts.First().IsOpen = !Flyouts.First().IsOpen;
        }
    }
}