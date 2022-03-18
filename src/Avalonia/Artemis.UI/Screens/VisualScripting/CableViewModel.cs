using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Exceptions;
using Artemis.UI.Screens.VisualScripting.Pins;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Extensions;
using Avalonia;
using Avalonia.Media;
using Avalonia.Threading;
using DynamicData;
using ReactiveUI;

namespace Artemis.UI.Screens.VisualScripting;

public class CableViewModel : ActivatableViewModelBase
{
    private const double CABLE_OFFSET = 24 * 4;

    private readonly NodeScriptViewModel _nodeScriptViewModel;
    private PinDirection _dragDirection;
    private Point _dragPoint;
    private bool _isDragging;
    private IPin? _from;
    private IPin? _to;
    private PinViewModel? _fromViewModel;
    private PinViewModel? _toViewModel;
    private readonly ObservableAsPropertyHelper<Point> _fromPoint;
    private readonly ObservableAsPropertyHelper<Point> _fromTargetPoint;
    private readonly ObservableAsPropertyHelper<Point> _toPoint;
    private readonly ObservableAsPropertyHelper<Point> _toTargetPoint;
    private readonly ObservableAsPropertyHelper<Color> _cableColor;

    public CableViewModel(NodeScriptViewModel nodeScriptViewModel, IPin? from, IPin? to)
    {
        if (from != null && from.Direction != PinDirection.Output)
            throw new ArtemisUIException("Can only create cables originating from an output pin");
        if (to != null && to.Direction != PinDirection.Input)
            throw new ArtemisUIException("Can only create cables targeted to an input pin");

        _nodeScriptViewModel = nodeScriptViewModel;
        _from = from;
        _to = to;

        this.WhenActivated(d =>
        {
            if (From != null)
                _nodeScriptViewModel.PinViewModels.Connect().Filter(p => p.Pin == From).Transform(model => FromViewModel = model).Subscribe().DisposeWith(d);
            if (To != null)
                _nodeScriptViewModel.PinViewModels.Connect().Filter(p => p.Pin == To).Transform(model => ToViewModel = model).Subscribe().DisposeWith(d);
        });

        _fromPoint = this.WhenAnyValue(vm => vm.FromViewModel).Select(p => p != null ? p.WhenAnyValue(pvm => pvm.Position) : Observable.Never<Point>()).Switch().ToProperty(this, vm => vm.FromPoint);
        _fromTargetPoint = this.WhenAnyValue(vm => vm.FromPoint).Select(point => new Point(point.X + CABLE_OFFSET, point.Y)).ToProperty(this, vm => vm.FromTargetPoint);
        _toPoint = this.WhenAnyValue(vm => vm.ToViewModel).Select(p => p != null ? p.WhenAnyValue(pvm => pvm.Position) : Observable.Never<Point>()).Switch().ToProperty(this, vm => vm.ToPoint);
        _toTargetPoint = this.WhenAnyValue(vm => vm.ToPoint).Select(point => new Point(point.X - CABLE_OFFSET, point.Y)).ToProperty(this, vm => vm.ToTargetPoint);
        _cableColor = this.WhenAnyValue(vm => vm.FromViewModel, vm => vm.ToViewModel).Select(tuple => tuple.Item1?.PinColor ?? tuple.Item2?.PinColor ?? new Color(255, 255, 255, 255)).ToProperty(this, vm => vm.CableColor);
    }

    public IPin? From
    {
        get => _from;
        set => RaiseAndSetIfChanged(ref _from, value);
    }

    public IPin? To
    {
        get => _to;
        set => RaiseAndSetIfChanged(ref _to, value);
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

    public bool IsDragging
    {
        get => _isDragging;
        set => RaiseAndSetIfChanged(ref _isDragging, value);
    }

    public PinDirection DragDirection
    {
        get => _dragDirection;
        set => RaiseAndSetIfChanged(ref _dragDirection, value);
    }

    public Point DragPoint
    {
        get => _dragPoint;
        set => RaiseAndSetIfChanged(ref _dragPoint, value);
    }

    public Point FromPoint => _fromPoint.Value;
    public Point FromTargetPoint => _fromTargetPoint.Value;
    public Point ToPoint => _toPoint.Value;
    public Point ToTargetPoint => _toTargetPoint.Value;
    public Color CableColor => _cableColor.Value;

    public void StartDrag(PinDirection dragDirection)
    {
        IsDragging = true;
        DragDirection = dragDirection;
    }

    public bool UpdateDrag(Point position, PinViewModel? targetViewModel)
    {
        DragPoint = position;
        return true;
    }

    public void FinishDrag()
    {
        IsDragging = false;
    }
}