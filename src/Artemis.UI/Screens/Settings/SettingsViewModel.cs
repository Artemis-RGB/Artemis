using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.Core.Services.Interfaces;
using Artemis.Core.Services.Storage.Interfaces;
using Artemis.Core.Utilities;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.Settings.Tabs.Devices;
using Artemis.UI.Screens.Settings.Tabs.Plugins;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Shared.Services.Interfaces;
using Artemis.UI.Shared.Utilities;
using Serilog.Events;
using Stylet;

namespace Artemis.UI.Screens.Settings
{
    public class SettingsViewModel : MainScreenViewModel
    {
        private readonly IDebugService _debugService;
        private readonly IDeviceSettingsVmFactory _deviceSettingsVmFactory;
        private readonly IDialogService _dialogService;
        private readonly IPluginService _pluginService;
        private readonly ISettingsService _settingsService;
        private readonly IPluginSettingsVmFactory _pluginSettingsVmFactory;
        private readonly ISurfaceService _surfaceService;
        private List<Tuple<string, int>> _targetFrameRates;
        private List<Tuple<string, double>> _renderScales;
        private List<int> _sampleSizes;
        private BindableCollection<DeviceSettingsViewModel> _deviceSettingsViewModels;
        private BindableCollection<PluginSettingsViewModel> _plugins;

        public SettingsViewModel(ISurfaceService surfaceService, IPluginService pluginService, IDialogService dialogService, IDebugService debugService,
            ISettingsService settingsService, IPluginSettingsVmFactory pluginSettingsVmFactory, IDeviceSettingsVmFactory deviceSettingsVmFactory)
        {
            DisplayName = "Settings";

            _surfaceService = surfaceService;
            _pluginService = pluginService;
            _dialogService = dialogService;
            _debugService = debugService;
            _settingsService = settingsService;
            _pluginSettingsVmFactory = pluginSettingsVmFactory;
            _deviceSettingsVmFactory = deviceSettingsVmFactory;

            DeviceSettingsViewModels = new BindableCollection<DeviceSettingsViewModel>();
            Plugins = new BindableCollection<PluginSettingsViewModel>();

            LogLevels = EnumUtilities.GetAllValuesAndDescriptions(typeof(LogEventLevel));
            ColorSchemes = EnumUtilities.GetAllValuesAndDescriptions(typeof(ApplicationColorScheme));
            RenderScales = new List<Tuple<string, double>> {new Tuple<string, double>("10%", 0.1)};
            for (var i = 25; i <= 100; i += 25)
                RenderScales.Add(new Tuple<string, double>(i + "%", i / 100.0));

            TargetFrameRates = new List<Tuple<string, int>>();
            for (var i = 10; i <= 30; i += 5)
                TargetFrameRates.Add(new Tuple<string, int>(i + " FPS", i));

            // Anything else is kinda broken right now
            SampleSizes = new List<int> {1, 9};
        }

        public List<Tuple<string, int>> TargetFrameRates
        {
            get => _targetFrameRates;
            set => SetAndNotify(ref _targetFrameRates, value);
        }

        public List<Tuple<string, double>> RenderScales
        {
            get => _renderScales;
            set => SetAndNotify(ref _renderScales, value);
        }

        public List<int> SampleSizes
        {
            get => _sampleSizes;
            set => SetAndNotify(ref _sampleSizes, value);
        }

        public BindableCollection<DeviceSettingsViewModel> DeviceSettingsViewModels
        {
            get => _deviceSettingsViewModels;
            set => SetAndNotify(ref _deviceSettingsViewModels, value);
        }

        public BindableCollection<PluginSettingsViewModel> Plugins
        {
            get => _plugins;
            set => SetAndNotify(ref _plugins, value);
        }

        public IEnumerable<ValueDescription> LogLevels { get; }
        public IEnumerable<ValueDescription> ColorSchemes { get; }

        public bool StartWithWindows
        {
            get => _settingsService.GetSetting("UI.AutoRun", false).Value;
            set
            {
                _settingsService.GetSetting("UI.AutoRun", false).Value = value;
                _settingsService.GetSetting("UI.AutoRun", false).Save();
                Task.Run(ApplyAutorun);
            }
        }

        public bool StartMinimized
        {
            get => !_settingsService.GetSetting("UI.ShowOnStartup", true).Value;
            set
            {
                _settingsService.GetSetting("UI.ShowOnStartup", true).Value = !value;
                _settingsService.GetSetting("UI.ShowOnStartup", true).Save();
            }
        }

        public bool ShowDataModelValues
        {
            get => _settingsService.GetSetting("ProfileEditor.ShowDataModelValues", false).Value;
            set
            {
                _settingsService.GetSetting("ProfileEditor.ShowDataModelValues", false).Value = value;
                _settingsService.GetSetting("ProfileEditor.ShowDataModelValues", false).Save();
            }
        }

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

        public ApplicationColorScheme SelectedColorScheme
        {
            get => _settingsService.GetSetting("UI.ColorScheme", ApplicationColorScheme.Automatic).Value;
            set
            {
                _settingsService.GetSetting("UI.ColorScheme", ApplicationColorScheme.Automatic).Value = value;
                _settingsService.GetSetting("UI.ColorScheme", ApplicationColorScheme.Automatic).Save();
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
            _debugService.ShowDebugger();
        }

        public async Task ShowLogsFolder()
        {
            try
            {
                Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs"));
            }
            catch (Exception e)
            {
                await _dialogService.ShowExceptionDialog("Welp, we couldn\'t open the logs folder for you", e);
            }
        }

        public async Task ShowDataFolder()
        {
            try
            {
                Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", Constants.DataFolder);
            }
            catch (Exception e)
            {
                await _dialogService.ShowExceptionDialog("Welp, we couldn\'t open the data folder for you", e);
            }
        }

        protected override void OnInitialActivate()
        {
            Task.Run(ApplyAutorun);

            DeviceSettingsViewModels.Clear();
            DeviceSettingsViewModels.AddRange(_surfaceService.ActiveSurface.Devices.Select(d => _deviceSettingsVmFactory.Create(d)));

            Plugins.Clear();
            Plugins.AddRange(_pluginService.GetAllPluginInfo().Select(p => _pluginSettingsVmFactory.Create(p.Instance)));

            base.OnInitialActivate();
        }

        protected override void OnClose()
        {
            DeviceSettingsViewModels.Clear();
            Plugins.Clear();
            base.OnClose();
        }

        private async Task ApplyAutorun()
        {
            try
            {
                var autoRunFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "Artemis.lnk");
                var executableFile = CurrentProcessUtilities.GetCurrentLocation();

                if (File.Exists(autoRunFile))
                    File.Delete(autoRunFile);
                if (StartWithWindows)
                    ShortcutUtilities.Create(autoRunFile, executableFile, "--autorun", new FileInfo(executableFile).DirectoryName, "Artemis", "", "");
            }
            catch (Exception e)
            {
                await _dialogService.ShowExceptionDialog("An exception occured while trying to apply the auto run setting", e);
                throw;
            }
        }
    }

    public enum ApplicationColorScheme
    {
        Light,
        Dark,
        Automatic
    }
}