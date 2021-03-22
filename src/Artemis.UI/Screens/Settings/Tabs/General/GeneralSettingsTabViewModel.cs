using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.LayerBrushes;
using Artemis.Core.Services;
using Artemis.UI.Screens.StartupWizard;
using Artemis.UI.Services;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Utilities;
using Ninject;
using Serilog.Events;
using Stylet;

namespace Artemis.UI.Screens.Settings.Tabs.General
{
    public class GeneralSettingsTabViewModel : Screen
    {
        private readonly IDebugService _debugService;
        private readonly PluginSetting<LayerBrushReference> _defaultLayerBrushDescriptor;
        private readonly IDialogService _dialogService;
        private readonly IKernel _kernel;
        private readonly IMessageService _messageService;
        private readonly IRegistrationService _registrationService;
        private readonly ISettingsService _settingsService;
        private readonly IUpdateService _updateService;
        private readonly IWindowManager _windowManager;
        private bool _canOfferUpdatesIfFound = true;
        private List<Tuple<string, double>> _renderScales;
        private List<Tuple<string, int>> _targetFrameRates;

        public GeneralSettingsTabViewModel(
            IKernel kernel,
            IWindowManager windowManager,
            IDialogService dialogService,
            IDebugService debugService,
            ISettingsService settingsService,
            IUpdateService updateService,
            IPluginManagementService pluginManagementService,
            IMessageService messageService,
            IRegistrationService registrationService)
        {
            DisplayName = "GENERAL";

            _kernel = kernel;
            _windowManager = windowManager;
            _dialogService = dialogService;
            _debugService = debugService;
            _settingsService = settingsService;
            _updateService = updateService;
            _messageService = messageService;
            _registrationService = registrationService;

            LogLevels = new BindableCollection<ValueDescription>(EnumUtilities.GetAllValuesAndDescriptions(typeof(LogEventLevel)));
            ColorSchemes = new BindableCollection<ValueDescription>(EnumUtilities.GetAllValuesAndDescriptions(typeof(ApplicationColorScheme)));
            RenderScales = new List<Tuple<string, double>> {new("10%", 0.1)};
            for (int i = 25; i <= 100; i += 25)
                RenderScales.Add(new Tuple<string, double>(i + "%", i / 100.0));

            TargetFrameRates = new List<Tuple<string, int>>();
            for (int i = 10; i <= 30; i += 5)
                TargetFrameRates.Add(new Tuple<string, int>(i + " FPS", i));

            List<LayerBrushProvider> layerBrushProviders = pluginManagementService.GetFeaturesOfType<LayerBrushProvider>();

            LayerBrushDescriptors = new BindableCollection<LayerBrushDescriptor>(layerBrushProviders.SelectMany(l => l.LayerBrushDescriptors));
            _defaultLayerBrushDescriptor = _settingsService.GetSetting("ProfileEditor.DefaultLayerBrushDescriptor", new LayerBrushReference
            {
                LayerBrushProviderId = "Artemis.Plugins.LayerBrushes.Color.ColorBrushProvider-92a9d6ba",
                BrushType = "SolidBrush"
            });

            WebServerPortSetting = _settingsService.GetSetting("WebServer.Port", 9696);
            WebServerPortSetting.AutoSave = true;
        }

        public BindableCollection<LayerBrushDescriptor> LayerBrushDescriptors { get; }

        public LayerBrushDescriptor SelectedLayerBrushDescriptor
        {
            get => LayerBrushDescriptors.FirstOrDefault(d => d.MatchesLayerBrushReference(_defaultLayerBrushDescriptor.Value));
            set
            {
                _defaultLayerBrushDescriptor.Value = new LayerBrushReference(value);
                _defaultLayerBrushDescriptor.Save();
            }
        }

        public BindableCollection<ValueDescription> LogLevels { get; }
        public BindableCollection<ValueDescription> ColorSchemes { get; }

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

        public bool StartWithWindows
        {
            get => _settingsService.GetSetting("UI.AutoRun", false).Value;
            set
            {
                _settingsService.GetSetting("UI.AutoRun", false).Value = value;
                _settingsService.GetSetting("UI.AutoRun", false).Save();
                NotifyOfPropertyChange(nameof(StartWithWindows));
                Task.Run(() => ApplyAutorun(false));
            }
        }

        public int AutoRunDelay
        {
            get => _settingsService.GetSetting("UI.AutoRunDelay", 15).Value;
            set
            {
                _settingsService.GetSetting("UI.AutoRunDelay", 15).Value = value;
                _settingsService.GetSetting("UI.AutoRunDelay", 15).Save();
                NotifyOfPropertyChange(nameof(AutoRunDelay));
                Task.Run(() => ApplyAutorun(true));
            }
        }

        public bool StartMinimized
        {
            get => !_settingsService.GetSetting("UI.ShowOnStartup", true).Value;
            set
            {
                _settingsService.GetSetting("UI.ShowOnStartup", true).Value = !value;
                _settingsService.GetSetting("UI.ShowOnStartup", true).Save();
                NotifyOfPropertyChange(nameof(StartMinimized));
            }
        }

        public bool CheckForUpdates
        {
            get => _settingsService.GetSetting("UI.CheckForUpdates", true).Value;
            set
            {
                _settingsService.GetSetting("UI.CheckForUpdates", true).Value = value;
                _settingsService.GetSetting("UI.CheckForUpdates", true).Save();
                NotifyOfPropertyChange(nameof(CheckForUpdates));

                if (!value)
                    AutoInstallUpdates = false;
            }
        }

        public bool AutoInstallUpdates
        {
            get => _settingsService.GetSetting("UI.AutoInstallUpdates", false).Value;
            set
            {
                _settingsService.GetSetting("UI.AutoInstallUpdates", false).Value = value;
                _settingsService.GetSetting("UI.AutoInstallUpdates", false).Save();
                NotifyOfPropertyChange(nameof(AutoInstallUpdates));
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

        public string PreferredGraphicsContext
        {
            get => _settingsService.GetSetting("Core.PreferredGraphicsContext", "Vulkan").Value;
            set
            {
                _settingsService.GetSetting("Core.PreferredGraphicsContext", "Vulkan").Value = value;
                _settingsService.GetSetting("Core.PreferredGraphicsContext", "Vulkan").Save();
                _registrationService.ApplyPreferredGraphicsContext();
            }
        }

        public double RenderScale
        {
            get => _settingsService.GetSetting("Core.RenderScale", 0.5).Value;
            set
            {
                _settingsService.GetSetting("Core.RenderScale", 0.5).Value = value;
                _settingsService.GetSetting("Core.RenderScale", 0.5).Save();
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

        public PluginSetting<int> WebServerPortSetting { get; }

        public bool CanOfferUpdatesIfFound
        {
            get => _canOfferUpdatesIfFound;
            set => SetAndNotify(ref _canOfferUpdatesIfFound, value);
        }

        public void ShowDebugger()
        {
            _debugService.ShowDebugger();
        }

        public void ShowLogsFolder()
        {
            try
            {
                Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", Path.Combine(Constants.DataFolder, "Logs"));
            }
            catch (Exception e)
            {
                _dialogService.ShowExceptionDialog("Welp, we couldn\'t open the logs folder for you", e);
            }
        }

        public void ShowSetupWizard()
        {
            _windowManager.ShowDialog(_kernel.Get<StartupWizardViewModel>());
        }

        public void ShowDataFolder()
        {
            try
            {
                Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", Constants.DataFolder);
            }
            catch (Exception e)
            {
                _dialogService.ShowExceptionDialog("Welp, we couldn\'t open the data folder for you", e);
            }
        }

        public async void OfferUpdatesIfFound()
        {
            if (!CanOfferUpdatesIfFound)
                return;

            CanOfferUpdatesIfFound = false;
            try
            {
                bool updateFound = await _updateService.OfferUpdateIfFound();
                if (!updateFound)
                    _messageService.ShowMessage("You are already running the latest Artemis build. (☞ﾟヮﾟ)☞");
            }
            catch (Exception exception)
            {
                _messageService.ShowMessage($"Failed to check for updates: {exception.Message}");
            }
            finally
            {
                CanOfferUpdatesIfFound = true;
            }
        }

        protected override void OnInitialActivate()
        {
            Task.Run(() => ApplyAutorun(false));
            base.OnInitialActivate();
        }

        private void ApplyAutorun(bool recreate)
        {
            if (!StartWithWindows)
                StartMinimized = false;

            // Remove the old auto-run method of placing a shortcut in shell:startup
            string autoRunFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "Artemis.lnk");
            if (File.Exists(autoRunFile))
                File.Delete(autoRunFile);

            if (Constants.BuildInfo.IsLocalBuild)
                return;

            // Create or remove the task if necessary
            try
            {
                bool taskCreated = false;
                if (!recreate) taskCreated = SettingsUtilities.IsAutoRunTaskCreated();

                if (StartWithWindows && !taskCreated)
                    SettingsUtilities.CreateAutoRunTask(TimeSpan.FromSeconds(AutoRunDelay));
                else if (!StartWithWindows && taskCreated)
                    SettingsUtilities.RemoveAutoRunTask();
            }
            catch (Exception e)
            {
                Execute.PostToUIThread(() => _dialogService.ShowExceptionDialog("An exception occured while trying to apply the auto run setting", e));
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