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
    private readonly ObservableAsPropertyHelper<Color> _cableColor;
    private readonly ObservableAsPropertyHelper<Point> _fromPoint;
    private readonly ObservableAsPropertyHelper<Point> _toPoint;

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
        });

        _fromPoint = this.WhenAnyValue(vm => vm.FromViewModel)
            .Select(p => p != null ? p.WhenAnyValue(pvm => pvm.Position) : Observable.Never<Point>())
            .Switch()
            .ToProperty(this, vm => vm.FromPoint);
        _toPoint = this.WhenAnyValue(vm => vm.ToViewModel)
            .Select(p => p != null ? p.WhenAnyValue(pvm => pvm.Position) : Observable.Never<Point>())
            .Switch()
            .ToProperty(this, vm => vm.ToPoint);

        _cableColor = this.WhenAnyValue(vm => vm.FromViewModel, vm => vm.ToViewModel)
            .Select(tuple => tuple.Item1?.PinColor ?? tuple.Item2?.PinColor ?? new Color(255, 255, 255, 255))
            .ToProperty(this, vm => vm.CableColor);
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

    public Point FromPoint => _fromPoint.Value;
    public Point ToPoint => _toPoint.Value;
    public Color CableColor => _cableColor.Value;
}