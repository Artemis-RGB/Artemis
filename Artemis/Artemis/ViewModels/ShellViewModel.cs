using System;
using System.Linq;
using Artemis.ViewModels.Abstract;
using Artemis.ViewModels.Flyouts;
using Caliburn.Micro;
using Ninject;

namespace Artemis.ViewModels
{
    public sealed class ShellViewModel : Conductor<IScreen>.Collection.OneActive
    {
        private readonly IKernel _kernel;

        public ShellViewModel(IKernel kernel, FlyoutSettingsViewModel flyoutSettings)
        {
            _kernel = kernel;

            // Setup UI
            DisplayName = "Artemis";

            Flyouts = new BindableCollection<FlyoutBaseViewModel>
            {
                flyoutSettings
            };
        }

        public IObservableCollection<FlyoutBaseViewModel> Flyouts { get; set; }

        protected override void OnActivate()
        {
            base.OnActivate();
            Items.Clear();

            var vms = _kernel.GetAll<BaseViewModel>();
            Items.AddRange(vms);
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            Items.Clear();
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