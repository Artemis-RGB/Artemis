using System.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Events;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.Home;
using Artemis.UI.Screens.Settings;
using Artemis.UI.Screens.Sidebar.Dialogs;
using Artemis.UI.Screens.SurfaceEditor;
using Artemis.UI.Screens.Workshop;
using Artemis.UI.Shared.Services;
using MaterialDesignThemes.Wpf;
using Ninject;
using RGB.NET.Core;
using Stylet;

namespace Artemis.UI.Screens.Sidebar
{
    public sealed class SidebarViewModel : Conductor<SidebarCategoryViewModel>.Collection.AllActive, IHandle<RequestSelectSidebarItemEvent>
    {
        private readonly IKernel _kernel;
        private readonly ISidebarVmFactory _sidebarVmFactory;
        private readonly IRgbService _rgbService;
        private readonly IProfileService _profileService;
        private readonly IDialogService _dialogService;
        private SidebarScreenViewModel _selectedSidebarScreen;
        private ArtemisDevice _headerDevice;
        private Screen _selectedScreen;

        public SidebarViewModel(IKernel kernel,
            IEventAggregator eventAggregator,
            ISidebarVmFactory sidebarVmFactory,
            IRgbService rgbService,
            IProfileService profileService,
            IDialogService dialogService)
        {
            _kernel = kernel;
            _sidebarVmFactory = sidebarVmFactory;
            _rgbService = rgbService;
            _profileService = profileService;
            _dialogService = dialogService;
            eventAggregator.Subscribe(this);

            SidebarScreens = new BindableCollection<SidebarScreenViewModel>
            {
                new SidebarScreenViewModel<HomeViewModel>(PackIconKind.Home, "Home"),
                new SidebarScreenViewModel<WorkshopViewModel>(PackIconKind.TestTube, "Workshop"),
                new SidebarScreenViewModel<SurfaceEditorViewModel>(PackIconKind.Devices, "Surface Editor"),
                new SidebarScreenViewModel<SettingsViewModel>(PackIconKind.Cog, "Settings")
            };
            SelectedSidebarScreen = SidebarScreens.First();
            UpdateProfileCategories();
            UpdateHeaderDevice();
        }

        private void UpdateHeaderDevice()
        {
            HeaderDevice = _rgbService.Devices.FirstOrDefault(d => d.DeviceType == RGBDeviceType.Keyboard && d.Layout.IsValid);
        }

        public ArtemisDevice HeaderDevice
        {
            get => _headerDevice;
            set => SetAndNotify(ref _headerDevice, value);
        }

        public BindableCollection<SidebarScreenViewModel> SidebarScreens { get; }

        public Screen SelectedScreen
        {
            get => _selectedScreen;
            private set => SetAndNotify(ref _selectedScreen, value);
        }

        public SidebarScreenViewModel SelectedSidebarScreen
        {
            get => _selectedSidebarScreen;
            set
            {
                if (SetAndNotify(ref _selectedSidebarScreen, value))
                    ActivateScreenViewModel(_selectedSidebarScreen);
            }
        }

        private void ActivateScreenViewModel(SidebarScreenViewModel screenViewModel)
        {
            SelectedScreen = screenViewModel.CreateInstance(_kernel);
        }

        private void UpdateProfileCategories()
        {
            foreach (ProfileCategory profileCategory in _profileService.ProfileCategories)
                AddProfileCategoryViewModel(profileCategory);
        }

        public async Task AddCategory()
        {
            object result = await _dialogService.ShowDialog<SidebarCategoryCreateViewModel>();
            if (result is ProfileCategory profileCategory)
                AddProfileCategoryViewModel(profileCategory);
        }

        public void OpenUrl(string url)
        {
            Core.Utilities.OpenUrl(url);
        }

        public void Handle(RequestSelectSidebarItemEvent message)
        {
            SidebarScreenViewModel requested = SidebarScreens.FirstOrDefault(s => s.DisplayName == message.DisplayName);
            if (requested != null)
                SelectedSidebarScreen = requested;
        }

        public SidebarCategoryViewModel AddProfileCategoryViewModel(ProfileCategory profileCategory)
        {
            SidebarCategoryViewModel viewModel = _sidebarVmFactory.SidebarCategoryViewModel(profileCategory);
            Items.Add(viewModel);
            return viewModel;
        }

        public void RemoveProfileCategoryViewModel(SidebarCategoryViewModel viewModel)
        {
            Items.Remove(viewModel);
        }

        #region Overrides of Screen

        /// <inheritdoc />
        protected override void OnInitialActivate()
        {
            base.OnInitialActivate();
        }

        /// <inheritdoc />
        protected override void OnClose()
        {
            base.OnClose();
        }

        #endregion
    }
}