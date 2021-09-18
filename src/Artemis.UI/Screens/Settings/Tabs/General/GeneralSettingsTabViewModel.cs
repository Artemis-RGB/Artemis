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

        public GeneralSettingsTabViewModel(
            IKernel kernel,
            IWindowManager windowManager,
            IDialogService dialogService,
            IDebugService debugService,
            ISettingsService settingsService,
            IUpdateService updateService,
            IPluginManagementService pluginManagementService,
            IRegistrationService registrationService,
            IMessageService messageService
        )
        {
            DisplayName = "GENERAL";

            _kernel = kernel;
            _windowManager = windowManager;
            _dialogService = dialogService;
            _debugService = debugService;
            _settingsService = settingsService;
            _updateService = updateService;
            _registrationService = registrationService;
            _messageService = messageService;

            LogLevels = new BindableCollection<ValueDescription>(EnumUtilities.GetAllValuesAndDescriptions(typeof(LogEventLevel)));
            ColorSchemes = new BindableCollection<ValueDescription>(EnumUtilities.GetAllValuesAndDescriptions(typeof(ApplicationColorScheme)));
            RenderScales = new BindableCollection<Tuple<string, double>>
            {
                new("25%", 0.25),
                new("50%", 0.5),
                new("100%", 1)
            };
            TargetFrameRates = new BindableCollection<Tuple<string, int>>
            {
                new("10 FPS", 10),
                new("20 FPS", 20),
                new("30 FPS", 30),
                new("45 FPS", 45),
                new("60 FPS (lol)", 60),
                new("144 FPS (omegalol)", 144)
            };

            List<LayerBrushProvider> layerBrushProviders = pluginManagementService.GetFeaturesOfType<LayerBrushProvider>();
            LayerBrushDescriptors = new BindableCollection<LayerBrushDescriptor>(layerBrushProviders.SelectMany(l => l.LayerBrushDescriptors));
            _defaultLayerBrushDescriptor = _settingsService.GetSetting("ProfileEditor.DefaultLayerBrushDescriptor", new LayerBrushReference
            {
                LayerBrushProviderId = "Artemis.Plugins.LayerBrushes.Color.ColorBrushProvider-92a9d6ba",
                BrushType = "SolidBrush"
            });
        }

        public BindableCollection<LayerBrushDescriptor> LayerBrushDescriptors { get; }

        public LayerBrushDescriptor SelectedLayerBrushDescriptor
        {
            get => LayerBrushDescriptors.FirstOrDefault(d => d.MatchesLayerBrushReference(_defaultLayerBrushDescriptor.Value));
            set => _defaultLayerBrushDescriptor.Value = new LayerBrushReference(value);
        }

        public BindableCollection<ValueDescription> LogLevels { get; }
        public BindableCollection<ValueDescription> ColorSchemes { get; }
        public BindableCollection<Tuple<string, double>> RenderScales { get; }
        public BindableCollection<Tuple<string, int>> TargetFrameRates { get; }

        public Tuple<string, double> SelectedRenderScale
        {
            get => RenderScales.FirstOrDefault(s => Math.Abs(s.Item2 - CoreRenderScale.Value) < 0.01);
            set => CoreRenderScale.Value = value.Item2;
        }

        public Tuple<string, int> SelectedTargetFrameRate
        {
            get => TargetFrameRates.FirstOrDefault(s => s.Item2 == CoreTargetFrameRate.Value);
            set => CoreTargetFrameRate.Value = value.Item2;
        }

        public PluginSetting<bool> UIAutoRun => _settingsService.GetSetting("UI.AutoRun", false);
        public PluginSetting<int> UIAutoRunDelay => _settingsService.GetSetting("UI.AutoRunDelay", 15);
        public PluginSetting<bool> UIShowOnStartup => _settingsService.GetSetting("UI.ShowOnStartup", true);
        public PluginSetting<bool> UICheckForUpdates => _settingsService.GetSetting("UI.CheckForUpdates", true);
        public PluginSetting<ApplicationColorScheme> UIColorScheme => _settingsService.GetSetting("UI.ColorScheme", ApplicationColorScheme.Automatic);
        public PluginSetting<bool> ProfileEditorShowDataModelValues => _settingsService.GetSetting("ProfileEditor.ShowDataModelValues", false);
        public PluginSetting<LogEventLevel> CoreLoggingLevel => _settingsService.GetSetting("Core.LoggingLevel", LogEventLevel.Information);
        public PluginSetting<string> CorePreferredGraphicsContext => _settingsService.GetSetting("Core.PreferredGraphicsContext", "Vulkan");
        public PluginSetting<double> CoreRenderScale => _settingsService.GetSetting("Core.RenderScale", 0.25);
        public PluginSetting<int> CoreTargetFrameRate => _settingsService.GetSetting("Core.TargetFrameRate", 30);
        public PluginSetting<int> WebServerPort => _settingsService.GetSetting("WebServer.Port", 9696);

        private void UIAutoRunOnSettingChanged(object sender, EventArgs e)
        {
            Task.Run(() => ApplyAutorun(false));
        }

        private void UIAutoRunDelayOnSettingChanged(object sender, EventArgs e)
        {
            Task.Run(() => ApplyAutorun(true));
        }

        private void CorePreferredGraphicsContextOnSettingChanged(object sender, EventArgs e)
        {
            _registrationService.ApplyPreferredGraphicsContext();
        }


        private void ApplyAutorun(bool recreate)
        {
            if (!UIAutoRun.Value)
                UIShowOnStartup.Value = true;

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
                if (!recreate)
                    taskCreated = SettingsUtilities.IsAutoRunTaskCreated();

                if (UIAutoRun.Value && !taskCreated)
                    SettingsUtilities.CreateAutoRunTask(TimeSpan.FromSeconds(UIAutoRunDelay.Value));
                else if (!UIAutoRun.Value && taskCreated)
                    SettingsUtilities.RemoveAutoRunTask();
            }
            catch (Exception e)
            {
                Execute.PostToUIThread(() => _dialogService.ShowExceptionDialog("An exception occured while trying to apply the auto run setting", e));
                throw;
            }
        }

        #region View methods

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

        #endregion

        #region Overrides of Screen

        protected override void OnInitialActivate()
        {
            Task.Run(() => ApplyAutorun(false));

            UIAutoRun.SettingChanged += UIAutoRunOnSettingChanged;
            UIAutoRunDelay.SettingChanged += UIAutoRunDelayOnSettingChanged;
            CorePreferredGraphicsContext.SettingChanged += CorePreferredGraphicsContextOnSettingChanged;
            base.OnInitialActivate();
        }

        protected override void OnClose()
        {
            UIAutoRun.SettingChanged -= UIAutoRunOnSettingChanged;
            UIAutoRunDelay.SettingChanged -= UIAutoRunDelayOnSettingChanged;
            CorePreferredGraphicsContext.SettingChanged -= CorePreferredGraphicsContextOnSettingChanged;

            _settingsService.SaveAllSettings();
            base.OnClose();
        }

        #endregion
    }

    public enum ApplicationColorScheme
    {
        Light,
        Dark,
        Automatic
    }
}