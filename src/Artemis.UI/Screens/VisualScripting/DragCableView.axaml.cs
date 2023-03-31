using System;
using System.Linq;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Artemis.UI.Screens.VisualScripting;

public partial class DragCableView : ReactiveUserControl<DragCableViewModel>
{
    private const double CABLE_OFFSET = 24 * 4;

    public DragCableView()
    {
        InitializeComponent();

        // Not using bindings here to avoid warnings
        this.WhenActivated(d =>
        {
            ViewModel.WhenAnyValue(vm => vm.FromPoint).Subscribe(_ => Update()).DisposeWith(d);
            ViewModel.WhenAnyValue(vm => vm.ToPoint).Subscribe(_ => Update()).DisposeWith(d);
            Update();
        });
    }


    private void Update()
    {
        PathFigure? pathFigure = ((PathGeometry) CablePath.Data).Figures?.FirstOrDefault();
        if (pathFigure?.Segments == null)
            return;
        
        BezierSegment segment = (BezierSegment) pathFigure.Segments.First();
        pathFigure.StartPoint = ViewModel!.FromPoint;
        segment.Point1 = new Point(ViewModel.FromPoint.X + CABLE_OFFSET, ViewModel.FromPoint.Y);
        segment.Point2 = new Point(ViewModel.ToPoint.X - CABLE_OFFSET, ViewModel.ToPoint.Y);
        segment.Point3 = new Point(ViewModel.ToPoint.X, ViewModel.ToPoint.Y);
    }
}