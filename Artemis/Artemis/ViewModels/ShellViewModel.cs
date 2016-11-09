using System;
using System.Linq;
using System.Windows;
using Artemis.Managers;
using Artemis.Services;
using Artemis.ViewModels.Abstract;
using Artemis.ViewModels.Flyouts;
using Caliburn.Micro;
using MahApps.Metro.Controls;
using Ninject;

namespace Artemis.ViewModels
{
    public sealed class ShellViewModel : Conductor<IScreen>.Collection.OneActive
    {
        private readonly IKernel _kernel;

        public ShellViewModel(IKernel kernel, MainManager mainManager, MetroDialogService metroDialogService,
            FlyoutSettingsViewModel flyoutSettings)
        {
            _kernel = kernel;


            // Setup UI
            DisplayName = "Artemis";

            Flyouts = new BindableCollection<FlyoutBaseViewModel>
            {
                flyoutSettings
            };


            MainManager = mainManager;
            MetroDialogService = metroDialogService;
            MainManager.EnableProgram();
        }

        public SystemTrayViewModel SystemTrayViewModel { get; set; }
        public MainManager MainManager { get; set; }
        public MetroDialogService MetroDialogService { get; set; }
        public IObservableCollection<FlyoutBaseViewModel> Flyouts { get; set; }

        private MetroWindow Window => (MetroWindow) GetView();

        public override void CanClose(Action<bool> callback)
        {
            if (Window.IsVisible)
                HideWindow();
            else
                ShowWindow();

            // ShellView is a strong and independent view who won't let herself get closed by the likes of anyone!
            callback(false);
        }

        public bool CanShowWindow => !Window.IsVisible;
        public bool CanHideWindow => Window.IsVisible;

        public void ShowWindow()
        {
            if (!Window.IsVisible)
                Window.Show();

            NotifyOfPropertyChange(() => CanShowWindow);
            NotifyOfPropertyChange(() => CanHideWindow);
        }

        public void HideWindow()
        {
            if (Window.IsVisible)
                Window.Hide();

            NotifyOfPropertyChange(() => CanShowWindow);
            NotifyOfPropertyChange(() => CanHideWindow);
        }

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