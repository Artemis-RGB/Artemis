using System.Windows.Media;
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
        private readonly IKernel _kernel;
        private readonly ICoreService _coreService;
        private readonly ISurfaceService _surfaceService;
        private readonly IWindowManager _windowManager;
        private readonly ISettingsService _settingsService;

        public SettingsViewModel(IKernel kernel, ICoreService coreService, ISurfaceService surfaceService, IWindowManager windowManager, ISettingsService settingsService)
        {
            _kernel = kernel;
            _coreService = coreService;
            _surfaceService = surfaceService;
            _windowManager = windowManager;
            _settingsService = settingsService;

            DeviceSettingsViewModels = new BindableCollection<DeviceSettingsViewModel>();
            RenderScale = _settingsService.GetSetting("RenderScale", 1.0).Value;
            TargetFrameRate = _settingsService.GetSetting("FrameRate", 25).Value;
        }

        public BindableCollection<DeviceSettingsViewModel> DeviceSettingsViewModels { get; set; }
        public string Title => "Settings";

        public double RenderScale { get; set; }
        public int TargetFrameRate { get; set; }

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