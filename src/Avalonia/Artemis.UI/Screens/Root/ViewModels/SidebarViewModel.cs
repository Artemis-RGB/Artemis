using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.Home.ViewModels;
using Artemis.UI.Screens.Settings;
using Artemis.UI.Screens.SurfaceEditor.ViewModels;
using Artemis.UI.Screens.Workshop.ViewModels;
using Artemis.UI.Shared;
using Material.Icons;
using Ninject;
using ReactiveUI;
using RGB.NET.Core;

namespace Artemis.UI.Screens.Root.ViewModels
{
    public class SidebarViewModel : ActivatableViewModelBase
    {
        private readonly IScreen _hostScreen;
        private readonly IProfileService _profileService;
        private readonly IRgbService _rgbService;
        private readonly ISidebarVmFactory _sidebarVmFactory;
        private ArtemisDevice? _headerDevice;

        private SidebarScreenViewModel? _selectedSidebarScreen;

        public SidebarViewModel(IScreen hostScreen, IKernel kernel, IProfileService profileService, IRgbService rgbService, ISidebarVmFactory sidebarVmFactory)
        {
            _hostScreen = hostScreen;
            _profileService = profileService;
            _rgbService = rgbService;
            _sidebarVmFactory = sidebarVmFactory;

            SidebarScreens = new ObservableCollection<SidebarScreenViewModel>
            {
                new SidebarScreenViewModel<HomeViewModel>(MaterialIconKind.Home, "Home"),
                new SidebarScreenViewModel<WorkshopViewModel>(MaterialIconKind.TestTube, "Workshop"),
                new SidebarScreenViewModel<SurfaceEditorViewModel>(MaterialIconKind.Devices, "Surface Editor"),
                new SidebarScreenViewModel<SettingsViewModel>(MaterialIconKind.Cog, "Settings")
            };
            _selectedSidebarScreen = SidebarScreens.First();

            UpdateProfileCategories();
            UpdateHeaderDevice();

            this.WhenActivated(disposables =>
            {
                this.WhenAnyObservable(vm => vm._hostScreen.Router.CurrentViewModel)
                    .WhereNotNull()
                    .Subscribe(c => SelectedSidebarScreen = SidebarScreens.FirstOrDefault(s => s.ScreenType == c.GetType()))
                    .DisposeWith(disposables);
                this.WhenAnyValue(vm => vm.SelectedSidebarScreen)
                    .WhereNotNull()
                    .Subscribe(s => _hostScreen.Router.Navigate.Execute(s.CreateInstance(kernel, _hostScreen)));
            });
        }

        public ObservableCollection<SidebarScreenViewModel> SidebarScreens { get; }
        public ObservableCollection<SidebarCategoryViewModel> SidebarCategories { get; } = new();

        public ArtemisDevice? HeaderDevice
        {
            get => _headerDevice;
            set => this.RaiseAndSetIfChanged(ref _headerDevice, value);
        }

        public SidebarScreenViewModel? SelectedSidebarScreen
        {
            get => _selectedSidebarScreen;
            set => this.RaiseAndSetIfChanged(ref _selectedSidebarScreen, value);
        }

        public SidebarCategoryViewModel AddProfileCategoryViewModel(ProfileCategory profileCategory)
        {
            SidebarCategoryViewModel viewModel = _sidebarVmFactory.SidebarCategoryViewModel(profileCategory);
            SidebarCategories.Add(viewModel);
            return viewModel;
        }

        private void UpdateProfileCategories()
        {
            SidebarCategories.Clear();
            foreach (ProfileCategory profileCategory in _profileService.ProfileCategories.OrderBy(p => p.Order))
                AddProfileCategoryViewModel(profileCategory);
        }

        private void UpdateHeaderDevice()
        {
            HeaderDevice = _rgbService.Devices.FirstOrDefault(d => d.DeviceType == RGBDeviceType.Keyboard && d.Layout is {IsValid: true});
        }
    }
}