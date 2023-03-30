using System;
using System.Linq;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Shapes;
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

            ViewModel.WhenAnyValue(vm => vm.FromPoint).Subscribe(_ => Update(true)).DisposeWith(d);
            ViewModel.WhenAnyValue(vm => vm.ToPoint).Subscribe(_ => Update(false)).DisposeWith(d);
            Update(true);
        });
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void Update(bool from)
    {
        // Workaround for https://github.com/AvaloniaUI/Avalonia/issues/4748
        CablePath.Margin = new Thickness(CablePath.Margin.Left + 1, CablePath.Margin.Top + 1, 0, 0);
        if (CablePath.Margin.Left > 2)
            CablePath.Margin = new Thickness(0, 0, 0, 0);

        PathFigure pathFigure = ((PathGeometry) CablePath.Data).Figures.First();
        BezierSegment segment = (BezierSegment) pathFigure.Segments!.First();
        pathFigure.StartPoint = ViewModel!.FromPoint;
        segment.Point1 = new Point(ViewModel.FromPoint.X + CABLE_OFFSET, ViewModel.FromPoint.Y);
        segment.Point2 = new Point(ViewModel.ToPoint.X - CABLE_OFFSET, ViewModel.ToPoint.Y);
        segment.Point3 = new Point(ViewModel.ToPoint.X, ViewModel.ToPoint.Y);

        Canvas.SetLeft(ValueBorder, ViewModel.FromPoint.X + (ViewModel.ToPoint.X - ViewModel.FromPoint.X) / 2);
        Canvas.SetTop(ValueBorder, ViewModel.FromPoint.Y + (ViewModel.ToPoint.Y - ViewModel.FromPoint.Y) / 2);
        
        CablePath.InvalidateVisual();
    }

    private void OnPointerEnter(object? sender, PointerEventArgs e)
    {
        ViewModel?.UpdateDisplayValue(true);
    }

    private void OnPointerLeave(object? sender, PointerEventArgs e)
    {
        ViewModel?.UpdateDisplayValue(false);
    }
}