using System;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.ProfileEditor;
using Avalonia;
using Avalonia.Controls.Mixins;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.VisualEditor.Visualizers;

public class LayerVisualizerViewModel : ActivatableViewModelBase, IVisualizerViewModel
{
    private ObservableAsPropertyHelper<bool>? _selected;

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
        });
    }

    public Layer Layer { get; }
    public bool Selected => _selected?.Value ?? false;
    public Rect LayerBounds => new(0, 0, Layer.Bounds.Width, Layer.Bounds.Height);

    private void Update()
    {
        this.RaisePropertyChanged(nameof(X));
        this.RaisePropertyChanged(nameof(Y));
        this.RaisePropertyChanged(nameof(LayerBounds));
    }

    public int X => Layer.Bounds.Left;
    public int Y => Layer.Bounds.Top;
    public int Order => 1;
}