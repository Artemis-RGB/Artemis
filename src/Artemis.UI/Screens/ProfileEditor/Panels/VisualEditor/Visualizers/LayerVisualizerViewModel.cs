using System;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Extensions;
using Artemis.UI.Shared.Services.ProfileEditor;
using Avalonia;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using SkiaSharp;

namespace Artemis.UI.Screens.ProfileEditor.VisualEditor.Visualizers;

public partial class LayerVisualizerViewModel : ActivatableViewModelBase, IVisualizerViewModel
{
    private ObservableAsPropertyHelper<bool>? _selected;
    [Notify] private Rect _layerBounds;
    [Notify] private double _x;
    [Notify] private double _y;

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
    
    private void Update()
    {
        SKRect bounds = Layer.GetLayerBounds();
        LayerBounds = new Rect(0, 0, bounds.Width, bounds.Height);
        X = bounds.Left;
        Y = bounds.Top;
    }

    public ProfileElement ProfileElement => Layer;
    
    public int Order => 1;
}