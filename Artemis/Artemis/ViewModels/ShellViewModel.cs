using System.Linq;
using Artemis.ViewModels.Abstract;
using Artemis.ViewModels.Flyouts;
using Caliburn.Micro;
using Ninject;

namespace Artemis.ViewModels
{
    public sealed class ShellViewModel : Conductor<IScreen>.Collection.OneActive
    {
        private readonly BaseViewModel[] _viewModels;

        public ShellViewModel(IKernel kernel, IEventAggregator events, BaseViewModel[] viewModels)
        {
            _viewModels = viewModels;

            events.Subscribe(this);

            // Setup UI
            DisplayName = "Artemis";
            Flyouts = new BindableCollection<FlyoutBaseViewModel>
            {
                kernel.Get<FlyoutSettingsViewModel>()
            };
        }

        public IObservableCollection<FlyoutBaseViewModel> Flyouts { get; set; }

        protected override void OnActivate()
        {
            base.OnActivate();
            foreach (var screen in _viewModels)
                ActivateItem(screen);

            ActiveItem = _viewModels.FirstOrDefault();
        }

        public void Settings()
        {
            Flyouts.First().IsOpen = !Flyouts.First().IsOpen;
        }

        public void CloseSettings()
        {
            Flyouts.First().IsOpen = false;
        }
    }
}