using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Media.Imaging;
using Artemis.Core;
using Artemis.Core.Services;
using SkiaSharp;
using SkiaSharp.Views.WPF;
using Stylet;

namespace Artemis.UI.Screens.Settings.Debug.Tabs.Performance
{
    public class PerformanceDebugViewModel : Conductor<PerformanceDebugPluginViewModel>.Collection.AllActive
    {
        private readonly IPluginManagementService _pluginManagementService;
        private readonly Timer _updateTimer;
        private readonly ICoreService _coreService;
        private readonly Timer _fpsTimer;
        private double _currentFps;
        private int _renderWidth;
        private int _renderHeight;
        private string _renderer;
        private int _frames;
        private double _delta;
        private double _lastDelta;

        public PerformanceDebugViewModel(ICoreService coreService, IPluginManagementService pluginManagementService)
        {
            _coreService = coreService;
            _pluginManagementService = pluginManagementService;
            _updateTimer = new Timer(500);
            _fpsTimer = new Timer(1000);
            
            DisplayName = "PERFORMANCE";
            _updateTimer.Elapsed += UpdateTimerOnElapsed;
            _fpsTimer.Start();
        }

        public double CurrentFps
        {
            get => _currentFps;
            set => SetAndNotify(ref _currentFps, value);
        }

        public int RenderWidth
        {
            get => _renderWidth;
            set => SetAndNotify(ref _renderWidth, value);
        }

        public int RenderHeight
        {
            get => _renderHeight;
            set => SetAndNotify(ref _renderHeight, value);
        }

        public string Renderer
        {
            get => _renderer;
            set => SetAndNotify(ref _renderer, value);
        }

        public double Delta
        {
            get => _delta;
            set => SetAndNotify(ref _delta, value);
        }

        private void UpdateTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            foreach (PerformanceDebugPluginViewModel viewModel in Items)
                viewModel.Update();
        }

        private void FeatureToggled(object sender, PluginFeatureEventArgs e)
        {
            Items.Clear();
            PopulateItems();
        }

        private void PluginToggled(object sender, PluginEventArgs e)
        {
            Items.Clear();
            PopulateItems();
        }

        private void PopulateItems()
        {
            Items.AddRange(_pluginManagementService.GetAllPlugins()
                .Where(p => p.IsEnabled && p.Profilers.Any(pr => pr.Measurements.Any()))
                .OrderBy(p => p.Info.Name)
                .Select(p => new PerformanceDebugPluginViewModel(p)));
        }

        #region Overrides of Screen

        /// <inheritdoc />
        protected override void OnActivate()
        {
            PopulateItems();
            _updateTimer.Start();
            _pluginManagementService.PluginDisabled += PluginToggled;
            _pluginManagementService.PluginDisabled += PluginToggled;
            _pluginManagementService.PluginFeatureEnabled += FeatureToggled;
            _pluginManagementService.PluginFeatureDisabled += FeatureToggled;
            _coreService.FrameRendering += CoreServiceOnFrameRendering;
            _coreService.FrameRendered += CoreServiceOnFrameRendered;
            _fpsTimer.Elapsed += FpsTimerOnElapsed;

            base.OnActivate();
        }

        /// <inheritdoc />
        protected override void OnDeactivate()
        {
            _updateTimer.Stop();
            _pluginManagementService.PluginDisabled -= PluginToggled;
            _pluginManagementService.PluginDisabled -= PluginToggled;
            _pluginManagementService.PluginFeatureEnabled -= FeatureToggled;
            _pluginManagementService.PluginFeatureDisabled -= FeatureToggled;
            _coreService.FrameRendering -= CoreServiceOnFrameRendering;
            _coreService.FrameRendered -= CoreServiceOnFrameRendered;
            _fpsTimer.Elapsed -= FpsTimerOnElapsed;

            Items.Clear();
            base.OnDeactivate();
        }

        /// <inheritdoc />
        protected override void OnClose()
        {
            _fpsTimer.Dispose();
            base.OnClose();
        }

        #endregion

        private void FpsTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            Delta = _lastDelta;
            CurrentFps = _frames;
            Renderer = Constants.ManagedGraphicsContext != null ? Constants.ManagedGraphicsContext.GetType().Name : "Software";
            _frames = 0;
        }

        private void CoreServiceOnFrameRendering(object? sender, FrameRenderingEventArgs e)
        {
            _lastDelta = e.DeltaTime;
        }

        private void CoreServiceOnFrameRendered(object sender, FrameRenderedEventArgs e)
        {
            _frames++;

            SKImageInfo bitmapInfo = e.Texture.ImageInfo;
            RenderHeight = bitmapInfo.Height;
            RenderWidth = bitmapInfo.Width;
        }
    }
}