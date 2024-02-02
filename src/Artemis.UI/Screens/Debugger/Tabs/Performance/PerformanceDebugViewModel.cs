using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using Avalonia.Threading;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using SkiaSharp;

namespace Artemis.UI.Screens.Debugger.Performance;

public partial class PerformanceDebugViewModel : ActivatableViewModelBase
{
    private readonly IRenderService _renderService;
    private readonly IPluginManagementService _pluginManagementService;
    private readonly DispatcherTimer _updateTimer;
    [Notify] private double _currentFps;
    [Notify] private string? _renderer;
    [Notify] private int _renderHeight;
    [Notify] private int _renderWidth;

    public PerformanceDebugViewModel(IRenderService renderService, IPluginManagementService pluginManagementService)
    {
        _renderService = renderService;
        _pluginManagementService = pluginManagementService;
        _updateTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(500), DispatcherPriority.Background, (_, _) => Update());

        DisplayName = "Performance";

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
            _updateTimer.Start();

            Disposable.Create(() =>
            {
                _updateTimer.Stop();
                Items.Clear();
                HandleDeactivation();
            }).DisposeWith(disposables);
        });
    }

    public ObservableCollection<PerformanceDebugPluginViewModel> Items { get; } = new();
    
    private void HandleActivation()
    {
        Renderer = _renderService.GraphicsContext?.GetType().Name ?? "Software";
        _renderService.FrameRendered += RenderServiceOnFrameRendered;
    }

    private void HandleDeactivation()
    {
        _renderService.FrameRendered -= RenderServiceOnFrameRendered;
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

    private void Update()
    {
        foreach (PerformanceDebugPluginViewModel viewModel in Items)
            viewModel.Update();
    }

    private void RenderServiceOnFrameRendered(object? sender, FrameRenderedEventArgs e)
    {
        CurrentFps = _renderService.FrameRate;
        SKImageInfo bitmapInfo = e.Texture.ImageInfo;

        RenderHeight = bitmapInfo.Height;
        RenderWidth = bitmapInfo.Width;
    }
}