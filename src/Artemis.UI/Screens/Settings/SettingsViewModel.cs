using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Services;
using Artemis.Core.Services.Storage.Interfaces;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.Settings.Debug;
using Artemis.UI.Screens.Settings.Tabs.Devices;
using MaterialDesignThemes.Wpf;
using Ninject;
using Stylet;

namespace Artemis.UI.Screens.Settings
{
    public class SettingsViewModel : MainScreenViewModel
    {
        private readonly IDeviceSettingsViewModelFactory _deviceSettingsViewModelFactory;
        private readonly IKernel _kernel;
        private readonly ISettingsService _settingsService;
        private readonly ISurfaceService _surfaceService;
        private readonly IWindowManager _windowManager;

        public SettingsViewModel(IKernel kernel,
            ISurfaceService surfaceService,
            IWindowManager windowManager,
            ISettingsService settingsService,
            IDeviceSettingsViewModelFactory deviceSettingsViewModelFactory)
        {
            DisplayName = "Settings";
            DisplayIcon = PackIconKind.Settings;
            DisplayOrder = 5;

            _kernel = kernel;
            _surfaceService = surfaceService;
            _windowManager = windowManager;
            _settingsService = settingsService;
            _deviceSettingsViewModelFactory = deviceSettingsViewModelFactory;

            DeviceSettingsViewModels = new BindableCollection<DeviceSettingsViewModel>();

            RenderScales = new List<Tuple<string, double>> {new Tuple<string, double>("10%", 0.1)};
            for (var i = 25; i <= 100; i += 25)
                RenderScales.Add(new Tuple<string, double>(i + "%", i / 100.0));

            TargetFrameRates = new List<Tuple<string, int>>();
            for (var i = 10; i <= 30; i += 5)
                TargetFrameRates.Add(new Tuple<string, int>(i + " FPS", i));
        }

        public BindableCollection<DeviceSettingsViewModel> DeviceSettingsViewModels { get; set; }

        public List<Tuple<string, double>> RenderScales { get; set; }

        public Tuple<string, double> SelectedRenderScale
        {
            get => RenderScales.FirstOrDefault(s => Math.Abs(s.Item2 - RenderScale) < 0.01);
            set => RenderScale = value.Item2;
        }

        public Tuple<string, int> SelectedTargetFrameRate
        {
            get => TargetFrameRates.FirstOrDefault(t => Math.Abs(t.Item2 - TargetFrameRate) < 0.01);
            set => TargetFrameRate = value.Item2;
        }

        public double RenderScale
        {
            get => _settingsService.GetSetting("Core.RenderScale", 1.0).Value;
            set
            {
                _settingsService.GetSetting("Core.RenderScale", 1.0).Value = value;
                _settingsService.GetSetting("Core.RenderScale", 1.0).Save();
            }
        }

        public List<Tuple<string, int>> TargetFrameRates { get; set; }

        public int TargetFrameRate
        {
            get => _settingsService.GetSetting("Core.TargetFrameRate", 25).Value;
            set
            {
                _settingsService.GetSetting("Core.TargetFrameRate", 25).Value = value;
                _settingsService.GetSetting("Core.TargetFrameRate", 25).Save();
            }
        }

        public string Title => "Settings";

        protected override void OnActivate()
        {
            DeviceSettingsViewModels.Clear();
            foreach (var device in _surfaceService.ActiveSurface.Devices)
                DeviceSettingsViewModels.Add(_deviceSettingsViewModelFactory.Create(device));

            base.OnActivate();
        }

        public void ShowDebugger()
        {
            _windowManager.ShowWindow(_kernel.Get<DebugViewModel>());
        }
    }
}