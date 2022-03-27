using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.UI.Exceptions;
using Artemis.UI.Screens.VisualScripting.Pins;
using Artemis.UI.Shared;
using Avalonia;
using Avalonia.Media;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;

namespace Artemis.UI.Screens.VisualScripting;

public class CableViewModel : ActivatableViewModelBase
{
    private readonly ObservableAsPropertyHelper<bool> _connected;
    private readonly ObservableAsPropertyHelper<Point> _fromPoint;
    private readonly ObservableAsPropertyHelper<Point> _toPoint;
    private readonly ObservableAsPropertyHelper<Point> _valuePoint;
    private ObservableAsPropertyHelper<Color>? _cableColor;

    private PinViewModel? _fromViewModel;
    private PinViewModel? _toViewModel;

    public CableViewModel(NodeScriptViewModel nodeScriptViewModel, IPin from, IPin to)
    {
        if (from.Direction != PinDirection.Output)
            throw new ArtemisUIException("Can only create cables originating from an output pin");
        if (to.Direction != PinDirection.Input)
            throw new ArtemisUIException("Can only create cables targeted to an input pin");


        this.WhenActivated(d =>
        {
            nodeScriptViewModel.PinViewModels.ToObservableChangeSet().Filter(p => ReferenceEquals(p.Pin, from)).Transform(model => FromViewModel = model).Subscribe().DisposeWith(d);
            nodeScriptViewModel.PinViewModels.ToObservableChangeSet().Filter(p => ReferenceEquals(p.Pin, to)).Transform(model => ToViewModel = model).Subscribe().DisposeWith(d);

            _cableColor = this.WhenAnyValue(vm => vm.FromViewModel, vm => vm.ToViewModel)
                .Select(tuple => tuple.Item1?.WhenAnyValue(p => p.PinColor) ?? tuple.Item2?.WhenAnyValue(p => p.PinColor) ?? Observable.Never<Color>())
                .Switch()
                .ToProperty(this, vm => vm.CableColor)
                .DisposeWith(d);
        });

        _fromPoint = this.WhenAnyValue(vm => vm.FromViewModel)
            .Select(p => p != null ? p.WhenAnyValue(pvm => pvm.Position) : Observable.Never<Point>())
            .Switch()
            .ToProperty(this, vm => vm.FromPoint);
        _toPoint = this.WhenAnyValue(vm => vm.ToViewModel)
            .Select(p => p != null ? p.WhenAnyValue(pvm => pvm.Position) : Observable.Never<Point>())
            .Switch()
            .ToProperty(this, vm => vm.ToPoint);
        _valuePoint = this.WhenAnyValue(vm => vm.FromPoint, vm => vm.ToPoint).Select(tuple => new Point(
            tuple.Item1.X + (tuple.Item2.X - tuple.Item1.X) / 2,
            tuple.Item1.Y + (tuple.Item2.Y - tuple.Item1.Y) / 2
        )).ToProperty(this, vm => vm.ValuePoint);

        // Not a perfect solution but this makes sure the cable never renders at 0,0 (can happen when the cable spawns before the pin ever rendered)
        _connected = this.WhenAnyValue(vm => vm.FromPoint, vm => vm.ToPoint)
            .Select(tuple => tuple.Item1 != new Point(0, 0) && tuple.Item2 != new Point(0, 0))
            .ToProperty(this, vm => vm.Connected);
    }

    public PinViewModel? FromViewModel
    {
        get => _fromViewModel;
        set => RaiseAndSetIfChanged(ref _fromViewModel, value);
    }

    public PinViewModel? ToViewModel
    {
        get => _toViewModel;
        set => RaiseAndSetIfChanged(ref _toViewModel, value);
    }

    public bool Connected => _connected.Value;

    public Point FromPoint => _fromPoint.Value;
    public Point ToPoint => _toPoint.Value;
    public Point ValuePoint => _valuePoint.Value;
    public Color CableColor => _cableColor?.Value ?? new Color(255, 255, 255, 255);
}