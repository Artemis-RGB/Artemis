using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.Home;
using Artemis.UI.Screens.ProfileEditor;
using Artemis.UI.Screens.Settings;
using Artemis.UI.Screens.SurfaceEditor;
using Artemis.UI.Screens.Workshop;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Builders;
using Artemis.UI.Shared.Services.ProfileEditor;
using Material.Icons;
using Ninject;
using ReactiveUI;
using RGB.NET.Core;

namespace Artemis.UI.Screens.Sidebar
{
    public class SidebarViewModel : ActivatableViewModelBase
    {
        private readonly IScreen _hostScreen;
        private readonly IProfileEditorService _profileEditorService;
        private readonly IProfileService _profileService;
        private readonly IRgbService _rgbService;
        private readonly ISidebarVmFactory _sidebarVmFactory;
        private readonly IWindowService _windowService;
        private ArtemisDevice? _headerDevice;

        private SidebarScreenViewModel? _selectedSidebarScreen;


        public SidebarViewModel(IScreen hostScreen, IKernel kernel, IProfileService profileService, IRgbService rgbService, IWindowService windowService,
            IProfileEditorService profileEditorService, ISidebarVmFactory sidebarVmFactory, IProfileEditorVmFactory profileEditorVmFactory)
        {
            _hostScreen = hostScreen;
            _profileService = profileService;
            _rgbService = rgbService;
            _windowService = windowService;
            _profileEditorService = profileEditorService;
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
                    .Subscribe(s =>
                    {
                        _hostScreen.Router.Navigate.Execute(s.CreateInstance(kernel, _hostScreen));
                        profileEditorService.ChangeCurrentProfileConfiguration(null);
                    });

                this.WhenAnyObservable(vm => vm._profileEditorService.ProfileConfiguration)
                    .Subscribe(profile =>
                    {
                        if (profile == null && _hostScreen.Router.GetCurrentViewModel() is ProfileEditorViewModel)
                            SelectedSidebarScreen = SidebarScreens.FirstOrDefault();
                        else if (profile != null && _hostScreen.Router.GetCurrentViewModel() is not ProfileEditorViewModel)
                            _hostScreen.Router.Navigate.Execute(profileEditorVmFactory.ProfileEditorViewModel(_hostScreen));
                    })
                    .DisposeWith(disposables);
            });
        }

        public ObservableCollection<SidebarScreenViewModel> SidebarScreens { get; }
        public ObservableCollection<SidebarCategoryViewModel> SidebarCategories { get; } = new();

        public ArtemisDevice? HeaderDevice
        {
            get => _headerDevice;
            set => RaiseAndSetIfChanged(ref _headerDevice, value);
        }

        public SidebarScreenViewModel? SelectedSidebarScreen
        {
            get => _selectedSidebarScreen;
            set => RaiseAndSetIfChanged(ref _selectedSidebarScreen, value);
        }

        public SidebarCategoryViewModel AddProfileCategoryViewModel(ProfileCategory profileCategory)
        {
            SidebarCategoryViewModel viewModel = _sidebarVmFactory.SidebarCategoryViewModel(this, profileCategory);
            SidebarCategories.Add(viewModel);
            return viewModel;
        }

        public async Task AddCategory()
        {
            await _windowService.CreateContentDialog()
                .WithTitle("Add new category")
                .WithViewModel(out SidebarCategoryEditViewModel vm, ("category", null))
                .HavingPrimaryButton(b => b.WithText("Confirm").WithCommand(vm.Confirm))
                .WithCloseButtonText("Cancel")
                .WithDefaultButton(ContentDialogButton.Primary)
                .ShowAsync();

            UpdateProfileCategories();
        }

        public void UpdateProfileCategories()
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