using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Artemis.Core;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Services;
using Artemis.Core.Services.Interfaces;
using Artemis.Core.Services.Storage.Interfaces;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.Settings.Debug;
using Artemis.UI.Screens.Settings.Tabs.Devices;
using Artemis.UI.Screens.Settings.Tabs.Plugins;
using Artemis.UI.Shared.Utilities;
using MaterialDesignThemes.Wpf;
using Ninject;
using Serilog.Events;
using Stylet;

namespace Artemis.UI.Screens.Settings
{
    public class SettingsViewModel : MainScreenViewModel
    {
        private readonly IDeviceSettingsVmFactory _deviceSettingsVmFactory;
        private readonly IKernel _kernel;
        private readonly IPluginService _pluginService;
        private readonly ISettingsService _settingsService;
        private readonly ISurfaceService _surfaceService;
        private readonly IWindowManager _windowManager;

        public SettingsViewModel(IKernel kernel,
            ISurfaceService surfaceService,
            IPluginService pluginService,
            IWindowManager windowManager,
            ISettingsService settingsService,
            IDeviceSettingsVmFactory deviceSettingsVmFactory)
        {
            DisplayName = "Settings";
            DisplayIcon = PackIconKind.Settings;
            DisplayOrder = 5;

            _kernel = kernel;
            _surfaceService = surfaceService;
            _pluginService = pluginService;
            _windowManager = windowManager;
            _settingsService = settingsService;
            _deviceSettingsVmFactory = deviceSettingsVmFactory;

            DeviceSettingsViewModels = new BindableCollection<DeviceSettingsViewModel>();
            Plugins = new BindableCollection<PluginSettingsViewModel>();

            LogLevels = EnumUtilities.GetAllValuesAndDescriptions(typeof(LogEventLevel));
            RenderScales = new List<Tuple<string, double>> {new Tuple<string, double>("10%", 0.1)};
            for (var i = 25; i <= 100; i += 25)
                RenderScales.Add(new Tuple<string, double>(i + "%", i / 100.0));

            TargetFrameRates = new List<Tuple<string, int>>();
            for (var i = 10; i <= 30; i += 5)
                TargetFrameRates.Add(new Tuple<string, int>(i + " FPS", i));

            // Anything else is kinda broken right now
            SampleSizes = new List<int> {1, 9};
        }

        public List<Tuple<string, int>> TargetFrameRates { get; set; }
        public List<Tuple<string, double>> RenderScales { get; set; }
        public IEnumerable<ValueDescription> LogLevels { get; private set; }

        public List<int> SampleSizes { get; set; }
        public BindableCollection<DeviceSettingsViewModel> DeviceSettingsViewModels { get; set; }
        public BindableCollection<PluginSettingsViewModel> Plugins { get; set; }

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

        public LogEventLevel SelectedLogLevel
        {
            get => _settingsService.GetSetting("Core.LoggingLevel", LogEventLevel.Information).Value;
            set
            {
                _settingsService.GetSetting("Core.LoggingLevel", LogEventLevel.Information).Value = value;
                _settingsService.GetSetting("Core.LoggingLevel", LogEventLevel.Information).Save();
            }
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

        public int TargetFrameRate
        {
            get => _settingsService.GetSetting("Core.TargetFrameRate", 25).Value;
            set
            {
                _settingsService.GetSetting("Core.TargetFrameRate", 25).Value = value;
                _settingsService.GetSetting("Core.TargetFrameRate", 25).Save();
            }
        }

        public int SampleSize
        {
            get => _settingsService.GetSetting("Core.SampleSize", 1).Value;
            set
            {
                _settingsService.GetSetting("Core.SampleSize", 1).Value = value;
                _settingsService.GetSetting("Core.SampleSize", 1).Save();
            }
        }

        public void ShowDebugger()
        {
            _windowManager.ShowWindow(_kernel.Get<DebugViewModel>());
        }

        public void ShowLogsFolder()
        {
            Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs"));
        }

        public void ShowDataFolder()
        {
            Process.Start(Constants.DataFolder);
        }

        protected override void OnActivate()
        {
            DeviceSettingsViewModels.Clear();
            foreach (var device in _surfaceService.ActiveSurface.Devices)
                DeviceSettingsViewModels.Add(_deviceSettingsVmFactory.Create(device));

            // TODO: GetPluginsOfType isn't ideal here as it doesn't include disabled plugins
            Plugins.Clear();
            foreach (var plugin in _pluginService.GetPluginsOfType<Plugin>())
                Plugins.Add(new PluginSettingsViewModel(plugin));

            base.OnActivate();
        }
    }
}