using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Services;
using Artemis.Core.Services.Interfaces;
using Artemis.Core.Services.Storage.Interfaces;
using Artemis.Core.Utilities;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.Settings.Debug;
using Artemis.UI.Screens.Settings.Tabs.Devices;
using Artemis.UI.Screens.Settings.Tabs.Plugins;
using Artemis.UI.Shared.Services.Interfaces;
using Artemis.UI.Shared.Utilities;
using Artemis.UI.Utilities;
using Microsoft.Win32;
using Ninject;
using Serilog.Events;
using Stylet;

namespace Artemis.UI.Screens.Settings
{
    public class SettingsViewModel : MainScreenViewModel
    {
        private readonly IDeviceSettingsVmFactory _deviceSettingsVmFactory;
        private readonly IDialogService _dialogService;
        private readonly IKernel _kernel;
        private readonly IPluginService _pluginService;
        private readonly ISettingsService _settingsService;
        private readonly ISurfaceService _surfaceService;
        private readonly IWindowManager _windowManager;

        public SettingsViewModel(IKernel kernel,
            ISurfaceService surfaceService,
            IPluginService pluginService,
            IDialogService dialogService,
            IWindowManager windowManager,
            ISettingsService settingsService,
            IDeviceSettingsVmFactory deviceSettingsVmFactory)
        {
            DisplayName = "Settings";

            _kernel = kernel;
            _surfaceService = surfaceService;
            _pluginService = pluginService;
            _dialogService = dialogService;
            _windowManager = windowManager;
            _settingsService = settingsService;
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

        public List<Tuple<string, int>> TargetFrameRates { get; set; }
        public List<Tuple<string, double>> RenderScales { get; set; }
        public IEnumerable<ValueDescription> LogLevels { get; }
        public IEnumerable<ValueDescription> ColorSchemes { get; }

        public List<int> SampleSizes { get; set; }
        public BindableCollection<DeviceSettingsViewModel> DeviceSettingsViewModels { get; set; }
        public BindableCollection<PluginSettingsViewModel> Plugins { get; set; }

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
            _windowManager.ShowWindow(_kernel.Get<DebugViewModel>());
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
            Plugins.AddRange(_pluginService.GetAllPluginInfo().Select(p => new PluginSettingsViewModel(p.Instance, _windowManager, _dialogService, _pluginService)));
            
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
                    ShortcutUtilities.Create(autoRunFile, executableFile, "-autorun", new FileInfo(executableFile).DirectoryName, "Artemis", "", "");
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