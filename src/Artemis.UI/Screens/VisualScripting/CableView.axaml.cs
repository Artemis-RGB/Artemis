using System;
using System.Linq;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Artemis.UI.Screens.VisualScripting;

public partial class CableView : ReactiveUserControl<CableViewModel>
{
    private const double CABLE_OFFSET = 24 * 4;

    public CableView()
    {
        InitializeComponent();

        // Not using bindings here to avoid a warnings
        this.WhenActivated(d =>
        {
            ValueBorder.GetObservable(BoundsProperty).Subscribe(rect => ValueBorder.RenderTransform = new TranslateTransform(rect.Width / 2 * -1, rect.Height / 2 * -1)).DisposeWith(d);

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

        Canvas.SetLeft(ValueBorder, ViewModel.FromPoint.X + (ViewModel.ToPoint.X - ViewModel.FromPoint.X) / 2);
        Canvas.SetTop(ValueBorder, ViewModel.FromPoint.Y + (ViewModel.ToPoint.Y - ViewModel.FromPoint.Y) / 2);
    }

    private void OnPointerEntered(object? sender, PointerEventArgs e)
    {
        ViewModel?.UpdateDisplayValue(true);
    }

    private void OnPointerExited(object? sender, PointerEventArgs e)
    {
        ViewModel?.UpdateDisplayValue(false);
    }
}