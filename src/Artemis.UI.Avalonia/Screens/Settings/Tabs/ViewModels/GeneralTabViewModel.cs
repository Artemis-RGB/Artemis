using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.LayerBrushes;
using Artemis.Core.Services;
using Artemis.UI.Avalonia.Services.Interfaces;
using ReactiveUI;
using Serilog.Events;

namespace Artemis.UI.Avalonia.Screens.Settings.Tabs.ViewModels
{
    public class GeneralTabViewModel : ActivatableViewModelBase
    {
        private readonly PluginSetting<LayerBrushReference> _defaultLayerBrushDescriptor;
        private readonly ISettingsService _settingsService;
        private readonly IDebugService _debugService;

        public GeneralTabViewModel(ISettingsService settingsService, IPluginManagementService pluginManagementService, IDebugService debugService)
        {
            DisplayName = "General";
            _settingsService = settingsService;
            _debugService = debugService;

            List<LayerBrushProvider> layerBrushProviders = pluginManagementService.GetFeaturesOfType<LayerBrushProvider>();
            LayerBrushDescriptors = new ObservableCollection<LayerBrushDescriptor>(layerBrushProviders.SelectMany(l => l.LayerBrushDescriptors));
            _defaultLayerBrushDescriptor = _settingsService.GetSetting("ProfileEditor.DefaultLayerBrushDescriptor", new LayerBrushReference
            {
                LayerBrushProviderId = "Artemis.Plugins.LayerBrushes.Color.ColorBrushProvider-92a9d6ba",
                BrushType = "SolidBrush"
            });

            ShowLogs = ReactiveCommand.Create(ExecuteShowLogs);
            CheckForUpdate = ReactiveCommand.CreateFromTask(ExecuteCheckForUpdate);
            ShowSetupWizard = ReactiveCommand.Create(ExecuteShowSetupWizard);
            ShowDebugger = ReactiveCommand.Create(ExecuteShowDebugger);
            ShowDataFolder = ReactiveCommand.Create(ExecuteShowDataFolder);
        }

        public ReactiveCommand<Unit, Unit> ShowLogs { get; }
        public ReactiveCommand<Unit, Unit> CheckForUpdate { get; }
        public ReactiveCommand<Unit, Unit> ShowSetupWizard { get; }
        public ReactiveCommand<Unit, Unit> ShowDebugger { get; }
        public ReactiveCommand<Unit, Unit> ShowDataFolder { get; }

        public ObservableCollection<LayerBrushDescriptor> LayerBrushDescriptors { get; }

        public ObservableCollection<string> GraphicsContexts { get; } = new()
        {
            "Software",
            "Vulkan"
        };

        public ObservableCollection<(string, double)> RenderScales { get; } = new()
        {
            new ValueTuple<string, double>("25%", 0.25),
            new ValueTuple<string, double>("50%", 0.5),
            new ValueTuple<string, double>("100%", 1)
        };

        public ObservableCollection<(string, int)> TargetFrameRates { get; } = new()
        {
            new ValueTuple<string, int>("10 FPS", 10),
            new ValueTuple<string, int>("20 FPS", 20),
            new ValueTuple<string, int>("30 FPS", 30),
            new ValueTuple<string, int>("45 FPS", 45),
            new ValueTuple<string, int>("60 FPS (lol)", 60),
            new ValueTuple<string, int>("144 FPS (omegalol)", 144)
        };

        public LayerBrushDescriptor? SelectedLayerBrushDescriptor
        {
            get => LayerBrushDescriptors.FirstOrDefault(d => d.MatchesLayerBrushReference(_defaultLayerBrushDescriptor.Value));
            set
            {
                if (value != null) _defaultLayerBrushDescriptor.Value = new LayerBrushReference(value);
            }
        }

        public (string, double)? SelectedRenderScale
        {
            get => RenderScales.FirstOrDefault(s => Math.Abs(s.Item2 - CoreRenderScale.Value) < 0.01);
            set
            {
                if (value != null) CoreRenderScale.Value = value.Value.Item2;
            }
        }

        public (string, int)? SelectedTargetFrameRate
        {
            get => TargetFrameRates.FirstOrDefault(s => s.Item2 == CoreTargetFrameRate.Value);
            set
            {
                if (value != null) CoreTargetFrameRate.Value = value.Value.Item2;
            }
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

        #region General

        private void ExecuteShowLogs()
        {
            OpenFolder(Path.Combine(Constants.DataFolder, "Logs"));
        }

        #endregion
        
        #region Updating

        private Task ExecuteCheckForUpdate(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        #endregion

        #region Tools

        private void ExecuteShowSetupWizard()
        {
        }

        private void ExecuteShowDebugger()
        {
            _debugService.ShowDebugger();
        }


        private void ExecuteShowDataFolder()
        {
            OpenFolder(Constants.DataFolder);
        }

        #endregion

        private void OpenFolder(string path)
        {
            if (OperatingSystem.IsWindows())
            {
                Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", path);
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