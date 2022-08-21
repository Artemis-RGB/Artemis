using System.Reactive.Disposables;
using Artemis.Core;
using Artemis.UI.Screens.VisualScripting.Pins;
using Artemis.UI.Shared;
using Avalonia;
using ReactiveUI;

namespace Artemis.UI.Screens.VisualScripting;

public class DragCableViewModel : ActivatableViewModelBase
{
    private Point _dragPoint;

    private ObservableAsPropertyHelper<Point>? _fromPoint;
    private ObservableAsPropertyHelper<Point>? _toPoint;

    public DragCableViewModel(PinViewModel pinViewModel)
    {
        PinViewModel = pinViewModel;

        // If the pin is output, the pin is the from-point and the drag position is the to-point
        if (PinViewModel.Pin.Direction == PinDirection.Output)
        {
            this.WhenActivated(d => { _fromPoint = PinViewModel.WhenAnyValue(vm => vm.Position).ToProperty(this, vm => vm.FromPoint).DisposeWith(d); });
            _toPoint = this.WhenAnyValue(vm => vm.DragPoint).ToProperty(this, vm => vm.ToPoint);
        }
        // If the pin is input, the pin is the to-point and the drag position is the from-point;
        else
        {
            this.WhenActivated(d => { _toPoint = PinViewModel.WhenAnyValue(vm => vm.Position).ToProperty(this, vm => vm.ToPoint).DisposeWith(d); });
            _fromPoint = this.WhenAnyValue(vm => vm.DragPoint).ToProperty(this, vm => vm.FromPoint);
        }
    }

    public PinViewModel PinViewModel { get; }
    public Point FromPoint => _fromPoint?.Value ?? new Point(0, 0);
    public Point ToPoint => _toPoint?.Value ?? new Point(0, 0);

    public Point DragPoint
    {
        get => _dragPoint;
        set => RaiseAndSetIfChanged(ref _dragPoint, value);
    }
}