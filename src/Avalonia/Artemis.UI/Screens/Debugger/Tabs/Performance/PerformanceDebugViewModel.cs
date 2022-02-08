using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Timers;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using ReactiveUI;
using SkiaSharp;

namespace Artemis.UI.Screens.Debugger.Performance
{
    public class PerformanceDebugViewModel : ActivatableViewModelBase, IRoutableViewModel
    {
        private readonly ICoreService _coreService;
        private readonly IPluginManagementService _pluginManagementService;
        private double _currentFps;
        private string? _renderer;
        private int _renderHeight;
        private int _renderWidth;

        public PerformanceDebugViewModel(IScreen hostScreen, ICoreService coreService, IPluginManagementService pluginManagementService)
        {
            HostScreen = hostScreen;
            _coreService = coreService;
            _pluginManagementService = pluginManagementService;

            Timer updateTimer = new(500);
            updateTimer.Elapsed += UpdateTimerOnElapsed;

            this.WhenActivated(disposables =>
            {
                Observable.FromEventPattern<PluginFeatureEventArgs>(x => pluginManagementService.PluginFeatureEnabled += x, x => pluginManagementService.PluginFeatureEnabled -= x)
                    .Subscribe(_ => Repopulate())
                    .DisposeWith(disposables);
                Observable.FromEventPattern<PluginFeatureEventArgs>(x => pluginManagementService.PluginFeatureDisabled += x, x => pluginManagementService.PluginFeatureDisabled -= x)
                    .Subscribe(_ => Repopulate())
                    .DisposeWith(disposables);
                Observable.FromEventPattern<PluginEventArgs>(x => pluginManagementService.PluginEnabled += x, x => pluginManagementService.PluginEnabled -= x)
                    .Subscribe(_ => Repopulate())
                    .DisposeWith(disposables);
                Observable.FromEventPattern<PluginEventArgs>(x => pluginManagementService.PluginDisabled += x, x => pluginManagementService.PluginDisabled -= x)
                    .Subscribe(_ => Repopulate())
                    .DisposeWith(disposables);

                HandleActivation();
                PopulateItems();
                updateTimer.Start();

                Disposable.Create(() =>
                {
                    updateTimer.Stop();
                    Items.Clear();
                    HandleDeactivation();
                }).DisposeWith(disposables);
            });
        }


        public string UrlPathSegment => "performance";
        public IScreen HostScreen { get; }
        public ObservableCollection<PerformanceDebugPluginViewModel> Items { get; } = new();

        public double CurrentFps
        {
            get => _currentFps;
            set => RaiseAndSetIfChanged(ref _currentFps, value);
        }

        public int RenderWidth
        {
            get => _renderWidth;
            set => RaiseAndSetIfChanged(ref _renderWidth, value);
        }

        public int RenderHeight
        {
            get => _renderHeight;
            set => RaiseAndSetIfChanged(ref _renderHeight, value);
        }

        public string? Renderer
        {
            get => _renderer;
            set => RaiseAndSetIfChanged(ref _renderer, value);
        }

        private void HandleActivation()
        {
            Renderer = Constants.ManagedGraphicsContext != null ? Constants.ManagedGraphicsContext.GetType().Name : "Software";
            _coreService.FrameRendered += CoreServiceOnFrameRendered;
        }

        private void HandleDeactivation()
        {
            _coreService.FrameRendered -= CoreServiceOnFrameRendered;
        }

        private void PopulateItems()
        {
            foreach (PerformanceDebugPluginViewModel performanceDebugPluginViewModel in _pluginManagementService.GetAllPlugins()
                         .Where(p => p.IsEnabled && p.Profilers.Any(pr => pr.Measurements.Any()))
                         .OrderBy(p => p.Info.Name)
                         .Select(p => new PerformanceDebugPluginViewModel(p)))
                Items.Add(performanceDebugPluginViewModel);
        }

        private void Repopulate()
        {
            Items.Clear();
            PopulateItems();
        }

        private void UpdateTimerOnElapsed(object? sender, ElapsedEventArgs e)
        {
            foreach (PerformanceDebugPluginViewModel viewModel in Items)
                viewModel.Update();
        }

        private void CoreServiceOnFrameRendered(object? sender, FrameRenderedEventArgs e)
        {
            CurrentFps = _coreService.FrameRate;
            SKImageInfo bitmapInfo = e.Texture.ImageInfo;

            RenderHeight = bitmapInfo.Height;
            RenderWidth = bitmapInfo.Width;
        }
    }
}