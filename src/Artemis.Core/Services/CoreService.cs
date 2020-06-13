﻿using System;
using System.Collections.Generic;
using Artemis.Core.Events;
using Artemis.Core.Exceptions;
using Artemis.Core.JsonConverters;
using Artemis.Core.Ninject;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services.Interfaces;
using Artemis.Core.Services.Storage.Interfaces;
using Artemis.Storage;
using Newtonsoft.Json;
using RGB.NET.Core;
using Serilog;
using Serilog.Events;
using SkiaSharp;

namespace Artemis.Core.Services
{
    /// <summary>
    ///     Provides Artemis's core update loop
    /// </summary>
    public class CoreService : ICoreService
    {
        private readonly ILogger _logger;
        private readonly IPluginService _pluginService;
        private readonly IProfileService _profileService;
        private readonly IRgbService _rgbService;
        private readonly ISurfaceService _surfaceService;
        private readonly PluginSetting<LogEventLevel> _loggingLevel;
        private List<Module> _modules;

        // ReSharper disable once UnusedParameter.Local - Storage migration service is injected early to ensure it runs before anything else
        internal CoreService(ILogger logger, StorageMigrationService _, ISettingsService settingsService, IPluginService pluginService,
            IRgbService rgbService, ISurfaceService surfaceService, IProfileService profileService)
        {
            _logger = logger;
            _pluginService = pluginService;
            _rgbService = rgbService;
            _surfaceService = surfaceService;
            _profileService = profileService;
            _loggingLevel = settingsService.GetSetting("Core.LoggingLevel", LogEventLevel.Debug);

            _rgbService.Surface.Updating += SurfaceOnUpdating;
            _rgbService.Surface.Updated += SurfaceOnUpdated;
            _loggingLevel.SettingChanged += (sender, args) => ApplyLoggingLevel();

            _modules = _pluginService.GetPluginsOfType<Module>();
            _pluginService.PluginEnabled += (sender, args) => _modules = _pluginService.GetPluginsOfType<Module>();
            _pluginService.PluginDisabled += (sender, args) => _modules = _pluginService.GetPluginsOfType<Module>();


            ConfigureJsonConvert();
        }

        public bool ModuleUpdatingDisabled { get; set; }
        public bool ModuleRenderingDisabled { get; set; }

        public void Dispose()
        {
            // Dispose services
            _pluginService.Dispose();
        }

        public bool IsInitialized { get; set; }

        public void Initialize()
        {
            if (IsInitialized)
                throw new ArtemisCoreException("Cannot initialize the core as it is already initialized.");

            _logger.Information("Initializing Artemis Core version {version}", typeof(CoreService).Assembly.GetName().Version);
            ApplyLoggingLevel();

            // Initialize the services
            _pluginService.CopyBuiltInPlugins();
            _pluginService.LoadPlugins();

            var surfaceConfig = _surfaceService.ActiveSurface;
            if (surfaceConfig != null)
                _logger.Information("Initialized with active surface entity {surfaceConfig}-{guid}", surfaceConfig.Name, surfaceConfig.EntityId);
            else
                _logger.Information("Initialized without an active surface entity");

            _profileService.ActivateDefaultProfiles();

            OnInitialized();
        }

        protected virtual void OnFrameRendering(FrameRenderingEventArgs e)
        {
            FrameRendering?.Invoke(this, e);
        }

        protected virtual void OnFrameRendered(FrameRenderedEventArgs e)
        {
            FrameRendered?.Invoke(this, e);
        }

        private void ConfigureJsonConvert()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> {new SKColorConverter()}
            };
        }

        private void ApplyLoggingLevel()
        {
            _logger.Information("Setting logging level to {loggingLevel}", _loggingLevel.Value);
            LoggerProvider.LoggingLevelSwitch.MinimumLevel = _loggingLevel.Value;
        }

        private void SurfaceOnUpdating(UpdatingEventArgs args)
        {
            try
            {
                if (!ModuleUpdatingDisabled && _modules != null)
                {
                    lock (_modules)
                    {
                        // Update all active modules
                        foreach (var module in _modules)
                            module.Update(args.DeltaTime);
                    }
                }

                // If there is no ready bitmap brush, skip the frame
                if (_rgbService.BitmapBrush == null)
                    return;

                lock (_rgbService.BitmapBrush)
                {
                    if (_rgbService.BitmapBrush.Bitmap == null)
                        return;

                    // Render all active modules
                    using (var canvas = new SKCanvas(_rgbService.BitmapBrush.Bitmap))
                    {
                        canvas.Clear(new SKColor(0, 0, 0));
                        if (!ModuleRenderingDisabled)
                        {
                            lock (_modules)
                            {
                                foreach (var module in _modules)
                                    module.Render(args.DeltaTime, _surfaceService.ActiveSurface, canvas, _rgbService.BitmapBrush.Bitmap.Info);
                            }
                        }

                        OnFrameRendering(new FrameRenderingEventArgs(_modules, canvas, args.DeltaTime, _rgbService.Surface));
                    }
                }
            }
            catch (Exception e)
            {
                throw new ArtemisCoreException("Exception during update", e);
            }
        }

        private void SurfaceOnUpdated(UpdatedEventArgs args)
        {
            OnFrameRendered(new FrameRenderedEventArgs(_rgbService.BitmapBrush, _rgbService.Surface));
        }

        #region Events

        public event EventHandler Initialized;
        public event EventHandler<FrameRenderingEventArgs> FrameRendering;
        public event EventHandler<FrameRenderedEventArgs> FrameRendered;

        private void OnInitialized()
        {
            IsInitialized = true;
            Initialized?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}