using System;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.ProfileEditor;
using Avalonia;
using Avalonia.Controls.Mixins;
using ReactiveUI;
using ShimSkiaSharp;

namespace Artemis.UI.Screens.ProfileEditor.VisualEditor.Visualizers;

public class LayerVisualizerViewModel : ActivatableViewModelBase, IVisualizerViewModel
{
    private ObservableAsPropertyHelper<bool>? _selected;
    private Rect _layerBounds;
    private double _x;
    private double _y;

    public LayerVisualizerViewModel(Layer layer, IProfileEditorService profileEditorService)
    {
        Layer = layer;
        this.WhenActivated(d =>
        {
            Observable.FromEventPattern(x => Layer.RenderPropertiesUpdated += x, x => Layer.RenderPropertiesUpdated -= x)
                .Subscribe(_ => Update())
                .DisposeWith(d);
            _selected = profileEditorService.ProfileElement
                .Select(p => p == Layer)
                .ToProperty(this, vm => vm.Selected)
                .DisposeWith(d);

            Update();
        });
    }

    public Layer Layer { get; }
    public bool Selected => _selected?.Value ?? false;

    public Rect LayerBounds
    {
        get => _layerBounds;
        private set => this.RaiseAndSetIfChanged(ref _layerBounds, value);
    }

    public double X
    {
        get => _x;
        set => this.RaiseAndSetIfChanged(ref _x, value);
    }

    public double Y
    {
        get => _y;
        set => this.RaiseAndSetIfChanged(ref _y, value);
    }

    public int Order => 1;

    private void Update()
    {
        // Create accurate bounds based on the RgbLeds and not the rounded ArtemisLeds
        SKPath path = new();
        foreach (ArtemisLed artemisLed in Layer.Leds)
        {
            path.AddRect(SKRect.Create(
                artemisLed.RgbLed.AbsoluteBoundary.Location.X,
                artemisLed.RgbLed.AbsoluteBoundary.Location.Y,
                artemisLed.RgbLed.AbsoluteBoundary.Size.Width,
                artemisLed.RgbLed.AbsoluteBoundary.Size.Height)
            );
        }

        LayerBounds = new Rect(0, 0, path.Bounds.Width, path.Bounds.Height);
        X = path.Bounds.Left;
        Y = path.Bounds.Top;
    }
}