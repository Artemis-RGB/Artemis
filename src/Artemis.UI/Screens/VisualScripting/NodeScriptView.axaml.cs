using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using Artemis.UI.Shared.Events;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using DynamicData.Binding;
using ReactiveUI;

namespace Artemis.UI.Screens.VisualScripting;

public partial class NodeScriptView : ReactiveUserControl<NodeScriptViewModel>
{
    public NodeScriptView()
    {
        InitializeComponent();

        NodeScriptZoomBorder.PropertyChanged += ZoomBorderOnPropertyChanged;
        UpdateZoomBorderBackground();
        
        NodeScriptZoomBorder.AddHandler(PointerReleasedEvent, CanvasOnPointerReleased, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble, true);
        NodeScriptZoomBorder.AddHandler(PointerWheelChangedEvent, ZoomOnPointerWheelChanged, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble, true);
        NodeScriptZoomBorder.AddHandler(PointerMovedEvent, ZoomOnPointerMoved, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble, true);
        
        this.WhenActivated(d =>
        {
            ViewModel!.AutoFitRequested += ViewModelOnAutoFitRequested;
            ViewModel.PickerPositionSubject.Subscribe(ShowPickerAt).DisposeWith(d);
            if (ViewModel.IsPreview)
            {
                BoundsProperty.Changed.Subscribe(BoundsPropertyChanged).DisposeWith(d);
                ViewModel.NodeViewModels.ToObservableChangeSet().Subscribe(_ => AutoFitIfPreview()).DisposeWith(d);
            }
        
            Dispatcher.UIThread.InvokeAsync(() => AutoFit(true), DispatcherPriority.ContextIdle);
            Disposable.Create(() => ViewModel.AutoFitRequested -= ViewModelOnAutoFitRequested).DisposeWith(d);
        });
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        AutoFitIfPreview();
        return base.MeasureOverride(availableSize);
    }

    private void ZoomOnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        // If scroll events aren't handled here the ZoomBorder does some random panning when at the zoom limit
        if (e.Delta.Y > 0 && NodeScriptZoomBorder.ZoomX >= 1)
            e.Handled = true;
    }

    private void ZoomOnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (ViewModel != null)
            ViewModel.PastePosition = e.GetPosition(ContainerGrid);
    }

    private void ShowPickerAt(Point point)
    {
        if (ViewModel == null)
            return;
        
        ((PopupFlyoutBase?) NodeScriptZoomBorder?.ContextFlyout)?.ShowAt(NodeScriptZoomBorder, true);
        ViewModel.NodePickerViewModel.Position = point;
    }

    private void AutoFitIfPreview()
    {
        if (ViewModel != null && ViewModel.IsPreview)
            AutoFit(true);
    }

    private void BoundsPropertyChanged(AvaloniaPropertyChangedEventArgs<Rect> obj)
    {
        if (NodesContainer.GetRealizedContainers().Contains(obj.Sender))
            AutoFitIfPreview();
    }

    private void CanvasOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        // If the flyout handled the click, update the position of the node picker
        if (e.Handled && ViewModel != null)
            ViewModel.NodePickerViewModel.Position = e.GetPosition(ContainerGrid);
    }

    private void AutoFit(bool skipTransitions)
    {
        List<Control> containers = NodesContainer.GetRealizedContainers().ToList();
        if (!containers.Any())
            return;

        double left = containers.Select(c => c.Bounds.Left).Min();
        double top = containers.Select(c => c.Bounds.Top).Min();
        double bottom = containers.Select(c => c.Bounds.Bottom).Max();
        double right = containers.Select(c => c.Bounds.Right).Max();

        // Add a 10 pixel margin around the rect
        Rect scriptRect = new(new Point(left - 10, top - 10), new Point(right + 10, bottom + 10));

        // The scale depends on the available space
        double scale = Math.Min(1, Math.Min(Bounds.Width / scriptRect.Width, Bounds.Height / scriptRect.Height));

        // Pan and zoom to make the script fit
        NodeScriptZoomBorder.Zoom(scale, 0, 0, skipTransitions);
        NodeScriptZoomBorder.Pan(Bounds.Center.X - scriptRect.Center.X * scale, Bounds.Center.Y - scriptRect.Center.Y * scale, skipTransitions);
    }

    private void ViewModelOnAutoFitRequested(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() => AutoFit(false));
    }

    private void ZoomBorderOnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name == nameof(NodeScriptZoomBorder.Background))
            UpdateZoomBorderBackground();
    }

    private void UpdateZoomBorderBackground()
    {
        if (NodeScriptZoomBorder.Background is VisualBrush visualBrush)
            visualBrush.DestinationRect = new RelativeRect(NodeScriptZoomBorder.OffsetX * -1, NodeScriptZoomBorder.OffsetY * -1, 20, 20, RelativeUnit.Absolute);
    }


    private void ZoomBorder_OnZoomChanged(object sender, ZoomChangedEventArgs e)
    {
        if (ViewModel != null)
            ViewModel.PanMatrix = NodeScriptZoomBorder.Matrix;
        UpdateZoomBorderBackground();
    }

    private void SelectionRectangle_OnSelectionUpdated(object? sender, SelectionRectangleEventArgs e)
    {
        List<Control> itemContainerInfos = NodesContainer.GetRealizedContainers().Where(c => c.Bounds.Intersects(e.Rectangle)).ToList();
        List<NodeViewModel> nodes = itemContainerInfos.Where(c => c.DataContext is NodeViewModel).Select(c => (NodeViewModel) c.DataContext!).ToList();
        ViewModel?.UpdateNodeSelection(nodes, e.KeyModifiers.HasFlag(KeyModifiers.Shift), e.KeyModifiers.HasFlag(KeyModifiers.Control));
    }

    private void SelectionRectangle_OnSelectionFinished(object? sender, SelectionRectangleEventArgs e)
    {
        ViewModel?.FinishNodeSelection();
    }

    private void ZoomBorder_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!SelectionRectangle.IsSelecting && e.InitialPressMouseButton == MouseButton.Left)
            ViewModel?.ClearNodeSelection();
    }
}