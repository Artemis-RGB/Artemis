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
        if (ViewModel == null)
            return;
        
        PathGeometry geometry = new()
        {
            Figures = new PathFigures()
        };
        PathFigure pathFigure = new()
        {
            StartPoint = ViewModel.FromPoint,
            IsClosed = false,
            Segments = new PathSegments
            {
                new BezierSegment
                {
                    Point1 = new Point(ViewModel.FromPoint.X + CABLE_OFFSET, ViewModel.FromPoint.Y),
                    Point2 = new Point(ViewModel.ToPoint.X - CABLE_OFFSET, ViewModel.ToPoint.Y),
                    Point3 = new Point(ViewModel.ToPoint.X, ViewModel.ToPoint.Y)
                }
            }
        };
        geometry.Figures.Add(pathFigure);
        CablePath.Data = geometry;
    }
}