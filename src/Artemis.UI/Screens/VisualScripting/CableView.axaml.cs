using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using Avalonia.Rendering;
using ReactiveUI;

namespace Artemis.UI.Screens.VisualScripting;

public class CableView : ReactiveUserControl<CableViewModel>
{
    private const double CABLE_OFFSET = 24 * 4;
    private readonly Path _cablePath;
    private readonly Border _valueBorder;

    public CableView()
    {
        InitializeComponent();
        _cablePath = this.Get<Path>("CablePath");
        _valueBorder = this.Get<Border>("ValueBorder");

        // Not using bindings here to avoid a warnings
        this.WhenActivated(d =>
        {
            _valueBorder.GetObservable(BoundsProperty).Subscribe(rect => _valueBorder.RenderTransform = new TranslateTransform(rect.Width / 2 * -1, rect.Height / 2 * -1)).DisposeWith(d);

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
        _cablePath.Margin = new Thickness(_cablePath.Margin.Left + 1, _cablePath.Margin.Top + 1, 0, 0);
        if (_cablePath.Margin.Left > 2)
            _cablePath.Margin = new Thickness(0, 0, 0, 0);

        PathFigure pathFigure = ((PathGeometry) _cablePath.Data).Figures.First();
        BezierSegment segment = (BezierSegment) pathFigure.Segments!.First();
        pathFigure.StartPoint = ViewModel!.FromPoint;
        segment.Point1 = new Point(ViewModel.FromPoint.X + CABLE_OFFSET, ViewModel.FromPoint.Y);
        segment.Point2 = new Point(ViewModel.ToPoint.X - CABLE_OFFSET, ViewModel.ToPoint.Y);
        segment.Point3 = new Point(ViewModel.ToPoint.X, ViewModel.ToPoint.Y);

        Canvas.SetLeft(_valueBorder, ViewModel.FromPoint.X + (ViewModel.ToPoint.X - ViewModel.FromPoint.X) / 2);
        Canvas.SetTop(_valueBorder, ViewModel.FromPoint.Y + (ViewModel.ToPoint.Y - ViewModel.FromPoint.Y) / 2);
        
        _cablePath.InvalidateVisual();
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