using System.Linq;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Events;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.Home;
using Artemis.UI.Screens.Settings;
using Artemis.UI.Screens.SurfaceEditor;
using Artemis.UI.Screens.Workshop;
using MaterialDesignThemes.Wpf;
using Ninject;
using RGB.NET.Core;
using Stylet;

namespace Artemis.UI.Screens.Sidebar
{
    public sealed class SidebarViewModel : Conductor<Screen>, IHandle<RequestSelectSidebarItemEvent>
    {
        private readonly IKernel _kernel;
        private readonly ISidebarVmFactory _sidebarVmFactory;
        private readonly IRgbService _rgbService;
        private SidebarScreenViewModel _selectedSidebarScreen;
        private ArtemisDevice _headerDevice;

        public SidebarViewModel(IKernel kernel, IEventAggregator eventAggregator, ISidebarVmFactory sidebarVmFactory, IRgbService rgbService)
        {
            _kernel = kernel;
            _sidebarVmFactory = sidebarVmFactory;
            _rgbService = rgbService;
            eventAggregator.Subscribe(this);

            SidebarScreens = new BindableCollection<SidebarScreenViewModel>
            {
                new SidebarScreenViewModel<HomeViewModel>(PackIconKind.Home, "Home"),
                new SidebarScreenViewModel<WorkshopViewModel>(PackIconKind.TestTube, "Workshop"),
                new SidebarScreenViewModel<SurfaceEditorViewModel>(PackIconKind.Devices, "Surface Editor"),
                new SidebarScreenViewModel<SettingsViewModel>(PackIconKind.Cog, "Settings")
            };
            SelectedSidebarScreen = SidebarScreens.First();
            ProfileCategories = new BindableCollection<SidebarCategoryViewModel>();
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
        public BindableCollection<SidebarCategoryViewModel> ProfileCategories { get; }

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
            ActiveItem = screenViewModel.CreateInstance(_kernel);
        }

        private void UpdateProfileCategories()
        {
            ProfileCategories.Add(_sidebarVmFactory.SidebarCategoryViewModel(new ProfileCategory("Test category 1")));
            ProfileCategories.Add(_sidebarVmFactory.SidebarCategoryViewModel(new ProfileCategory("Test category 2")));
            ProfileCategories.Add(_sidebarVmFactory.SidebarCategoryViewModel(new ProfileCategory("Test category 3")));
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