using Artemis.Core.Services;
using Artemis.Core.Services.Interfaces;
using Artemis.Core.Services.Storage;
using Artemis.UI.ViewModels.Controls.Settings;
using Artemis.UI.ViewModels.Interfaces;
using Ninject;
using Stylet;

namespace Artemis.UI.ViewModels.Screens
{
    public class SettingsViewModel : Screen, ISettingsViewModel
    {
        private readonly ICoreService _coreService;
        private readonly IKernel _kernel;
        private readonly ISettingsService _settingsService;
        private readonly ISurfaceService _surfaceService;
        private readonly IWindowManager _windowManager;

        public SettingsViewModel(IKernel kernel, ICoreService coreService, ISurfaceService surfaceService, IWindowManager windowManager, ISettingsService settingsService)
        {
            _kernel = kernel;
            _coreService = coreService;
            _surfaceService = surfaceService;
            _windowManager = windowManager;
            _settingsService = settingsService;

            DeviceSettingsViewModels = new BindableCollection<DeviceSettingsViewModel>();
        }

        public BindableCollection<DeviceSettingsViewModel> DeviceSettingsViewModels { get; set; }

        public double RenderScale
        {
            get => _settingsService.GetSetting("RenderScale", 1.0).Value;
            set
            {
                _settingsService.GetSetting("RenderScale", 1.0).Value = value;
                _settingsService.GetSetting("RenderScale", 1.0).Save();
            }
        }

        public int TargetFrameRate
        {
            get => _settingsService.GetSetting("TargetFrameRate", 25).Value;
            set
            {
                _settingsService.GetSetting("TargetFrameRate", 25).Value = value;
                _settingsService.GetSetting("TargetFrameRate", 25).Save();
            }
        }

        public string Title => "Settings";

        protected override void OnActivate()
        {
            DeviceSettingsViewModels.Clear();
            foreach (var device in _surfaceService.ActiveSurface.Devices)
                DeviceSettingsViewModels.Add(new DeviceSettingsViewModel(device, _coreService));

            base.OnActivate();
        }

        public void ShowDebugger()
        {
            _windowManager.ShowWindow(_kernel.Get<DebugViewModel>());
        }
    }
}