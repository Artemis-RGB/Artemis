using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Events;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Events;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using DynamicData.Binding;
using ReactiveUI;

namespace Artemis.UI.Screens.VisualScripting;

public class NodeScriptView : ReactiveUserControl<NodeScriptViewModel>
{
    private readonly ItemsControl _nodesContainer;
    private readonly SelectionRectangle _selectionRectangle;
    private readonly ZoomBorder _zoomBorder;
    private readonly Grid _grid;

    public NodeScriptView()
    {
        InitializeComponent();

        _grid = this.Find<Grid>("ContainerGrid");
        _zoomBorder = this.Find<ZoomBorder>("ZoomBorder");
        _nodesContainer = this.Find<ItemsControl>("NodesContainer");
        _selectionRectangle = this.Find<SelectionRectangle>("SelectionRectangle");
        _zoomBorder.PropertyChanged += ZoomBorderOnPropertyChanged;
        UpdateZoomBorderBackground();

        _zoomBorder.AddHandler(PointerReleasedEvent, CanvasOnPointerReleased, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble, true);
        _zoomBorder.AddHandler(PointerWheelChangedEvent, ZoomOnPointerWheelChanged, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble, true);
        this.WhenActivated(d =>
        {
            ViewModel.AutoFitRequested += ViewModelOnAutoFitRequested;
            ViewModel!.PickerPositionSubject.Subscribe(ShowPickerAt).DisposeWith(d);
            if (ViewModel.IsPreview)
            {
                BoundsProperty.Changed.Subscribe(BoundsPropertyChanged).DisposeWith(d);
                ViewModel.NodeViewModels.ToObservableChangeSet().Subscribe(_ => AutoFitIfPreview()).DisposeWith(d);
            }

            Dispatcher.UIThread.InvokeAsync(() => AutoFit(true), DispatcherPriority.ContextIdle);
            Disposable.Create(() => ViewModel.AutoFitRequested -= ViewModelOnAutoFitRequested).DisposeWith(d);
        });
    }

    private void ZoomOnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        // If scroll events aren't handled here the ZoomBorder does some random panning when at the zoom limit
        if (e.Delta.Y > 0 && _zoomBorder.ZoomX >= 1)
            e.Handled = true;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        AutoFitIfPreview();
        return base.MeasureOverride(availableSize);
    }

    private void ShowPickerAt(Point point)
    {
        if (ViewModel == null)
            return;
        ViewModel.NodePickerViewModel.Position = point;
        _zoomBorder?.ContextFlyout?.ShowAt(_zoomBorder, true);
    }

    private void AutoFitIfPreview()
    {
        if (ViewModel != null && ViewModel.IsPreview)
            AutoFit(true);
    }

    private void BoundsPropertyChanged(AvaloniaPropertyChangedEventArgs<Rect> obj)
    {
        if (_nodesContainer.ItemContainerGenerator.Containers.Select(c => c.ContainerControl).Contains(obj.Sender))
            AutoFitIfPreview();
    }

    private void CanvasOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        // If the flyout handled the click, update the position of the node picker
        if (e.Handled && ViewModel != null)
            ViewModel.NodePickerViewModel.Position = e.GetPosition(_grid);
    }

    private void AutoFit(bool skipTransitions)
    {
        if (!_nodesContainer.ItemContainerGenerator.Containers.Any())
            return;

        double left = _nodesContainer.ItemContainerGenerator.Containers.Select(c => c.ContainerControl.Bounds.Left).Min();
        double top = _nodesContainer.ItemContainerGenerator.Containers.Select(c => c.ContainerControl.Bounds.Top).Min();
        double bottom = _nodesContainer.ItemContainerGenerator.Containers.Select(c => c.ContainerControl.Bounds.Bottom).Max();
        double right = _nodesContainer.ItemContainerGenerator.Containers.Select(c => c.ContainerControl.Bounds.Right).Max();

        // Add a 10 pixel margin around the rect
        Rect scriptRect = new(new Point(left - 10, top - 10), new Point(right + 10, bottom + 10));

        // The scale depends on the available space
        double scale = Math.Min(1, Math.Min(Bounds.Width / scriptRect.Width, Bounds.Height / scriptRect.Height));

        // Pan and zoom to make the script fit
        _zoomBorder.Zoom(scale, 0, 0, skipTransitions);
        _zoomBorder.Pan(Bounds.Center.X - scriptRect.Center.X * scale, Bounds.Center.Y - scriptRect.Center.Y * scale, skipTransitions);
    }

    private void ViewModelOnAutoFitRequested(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() => AutoFit(false));
    }

    private void ZoomBorderOnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name == nameof(_zoomBorder.Background))
            UpdateZoomBorderBackground();
    }

    private void UpdateZoomBorderBackground()
    {
        if (_zoomBorder.Background is VisualBrush visualBrush)
            visualBrush.DestinationRect = new RelativeRect(_zoomBorder.OffsetX * -1, _zoomBorder.OffsetY * -1, 20, 20, RelativeUnit.Absolute);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void ZoomBorder_OnZoomChanged(object sender, ZoomChangedEventArgs e)
    {
        if (ViewModel != null)
            ViewModel.PanMatrix = _zoomBorder.Matrix;
        UpdateZoomBorderBackground();
    }

    private void SelectionRectangle_OnSelectionUpdated(object? sender, SelectionRectangleEventArgs e)
    {
        List<ItemContainerInfo> itemContainerInfos = _nodesContainer.ItemContainerGenerator.Containers.Where(c => c.ContainerControl.Bounds.Intersects(e.Rectangle)).ToList();
        List<NodeViewModel> nodes = itemContainerInfos.Where(c => c.Item is NodeViewModel).Select(c => (NodeViewModel) c.Item).ToList();
        ViewModel?.UpdateNodeSelection(nodes, e.KeyModifiers.HasFlag(KeyModifiers.Shift), e.KeyModifiers.HasFlag(KeyModifiers.Control));
    }

    private void SelectionRectangle_OnSelectionFinished(object? sender, SelectionRectangleEventArgs e)
    {
        ViewModel?.FinishNodeSelection();
    }

    private void ZoomBorder_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_selectionRectangle.IsSelecting && e.InitialPressMouseButton == MouseButton.Left)
            ViewModel?.ClearNodeSelection();
    }
}