using System;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Extensions;
using Artemis.UI.Shared.Services.ProfileEditor;
using Avalonia;
using Avalonia.Controls.Mixins;
using ReactiveUI;
using SkiaSharp;

namespace Artemis.UI.Screens.ProfileEditor.VisualEditor.Visualizers;

public class LayerVisualizerViewModel : ActivatableViewModelBase, IVisualizerViewModel
{
    private Rect _layerBounds;
    private ObservableAsPropertyHelper<bool>? _selected;
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
        private set => RaiseAndSetIfChanged(ref _layerBounds, value);
    }

    private void Update()
    {
        SKRect bounds = Layer.GetLayerBounds();
        LayerBounds = new Rect(0, 0, bounds.Width, bounds.Height);
        X = bounds.Left;
        Y = bounds.Top;
    }

    public ProfileElement ProfileElement => Layer;

    public double X
    {
        get => _x;
        set => RaiseAndSetIfChanged(ref _x, value);
    }

    public double Y
    {
        get => _y;
        set => RaiseAndSetIfChanged(ref _y, value);
    }

    public int Order => 1;
}