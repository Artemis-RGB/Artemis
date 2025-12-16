using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using ReactiveUI.Avalonia;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Artemis.UI.Screens.VisualScripting.Pins;

public class PinView : ReactiveUserControl<PinViewModel>
{
    private readonly DispatcherTimer _updateTimer;
    private bool _dragging;
    private Canvas? _container;
    private Border? _pinPoint;

    public PinView()
    {
        _updateTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(16), DispatcherPriority.Render, UpdatePosition);
    }

    protected void InitializePin(Border pinPoint)
    {
        _pinPoint = pinPoint;
        _pinPoint.PointerMoved += PinPointOnPointerMoved;
        _pinPoint.PointerReleased += PinPointOnPointerReleased;
    }
    
    private void PinPointOnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (ViewModel == null || _container == null || _pinPoint == null)
            return;

        NodeScriptViewModel? nodeScriptViewModel = this.FindAncestorOfType<NodeScriptView>()?.ViewModel;
        PointerPoint point = e.GetCurrentPoint(_container);
        if (nodeScriptViewModel == null || !point.Properties.IsLeftButtonPressed)
            return;

        if (!_dragging)
            e.Pointer.Capture(_pinPoint);

        PinViewModel? targetPin = (_container.InputHitTest(point.Position) as Border)?.DataContext as PinViewModel;
        if (targetPin == ViewModel)
            targetPin = null;

        _pinPoint.Cursor = new Cursor(nodeScriptViewModel.UpdatePinDrag(ViewModel, targetPin, point.Position) ? StandardCursorType.Hand : StandardCursorType.No);
        _dragging = true;
        e.Handled = true;
    }

    private void PinPointOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_dragging || ViewModel == null || _container == null || _pinPoint == null)
            return;

        _dragging = false;
        e.Pointer.Capture(null);

        PointerPoint point = e.GetCurrentPoint(_container);
        PinViewModel? targetPin = (_container.InputHitTest(point.Position) as Border)?.DataContext as PinViewModel;
        if (targetPin == ViewModel)
            targetPin = null;

        this.FindAncestorOfType<NodeScriptView>()?.ViewModel?.FinishPinDrag(ViewModel, targetPin, point.Position);
        _pinPoint.Cursor = new Cursor(StandardCursorType.Hand);
        e.Handled = true;
    }

    #region Overrides of Visual

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _container = this.FindAncestorOfType<Canvas>();
        _updateTimer.Start();
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _updateTimer.Stop();
    }

    public void UpdatePosition(object? sender, EventArgs eventArgs)
    {
        if (_container == null || _pinPoint == null || ViewModel == null)
            return;

        Matrix? transform = _pinPoint.TransformToVisual(_container);
        if (transform != null)
            ViewModel.Position = new Point(_pinPoint.Bounds.Width / 2, _pinPoint.Bounds.Height / 2).Transform(transform.Value);
    }

    #endregion
}