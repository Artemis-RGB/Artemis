using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.VisualEditor.Tools;

public class TransformToolView : ReactiveUserControl<TransformToolViewModel>
{
    private ZoomBorder? _zoomBorder;
    private PointerPoint _dragOffset;

    public TransformToolView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    #region Zoom

    /// <inheritdoc />
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        _zoomBorder = (ZoomBorder?)this.GetLogicalAncestors().FirstOrDefault(l => l is ZoomBorder);
        if (_zoomBorder != null)
            _zoomBorder.PropertyChanged += ZoomBorderOnPropertyChanged;
        base.OnAttachedToLogicalTree(e);
    }

    /// <inheritdoc />
    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        if (_zoomBorder != null)
            _zoomBorder.PropertyChanged -= ZoomBorderOnPropertyChanged;
        base.OnDetachedFromLogicalTree(e);
    }

    private void ZoomBorderOnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != ZoomBorder.ZoomXProperty || _zoomBorder == null)
            return;

        // TODO
    }

    #endregion

    #region Rotation

    private void RotationOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        IInputElement? element = (IInputElement?)sender;
        if (element == null || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        _dragOffset = e.GetCurrentPoint(_zoomBorder);

        e.Pointer.Capture(element);
        e.Handled = true;
    }

    private void RotationOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        IInputElement? element = (IInputElement?)sender;
        if (element == null || e.Pointer.Captured != element || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        e.Pointer.Capture(null);
        e.Handled = true;
    }

    private void RotationOnPointerMoved(object? sender, PointerEventArgs e)
    {
        IInputElement? element = (IInputElement?) sender;
        if (element == null || e.Pointer.Captured != element || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        e.Handled = true;
    }

    #endregion

    #region Movement

    private void MoveOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        IInputElement? element = (IInputElement?)sender;
        if (element == null || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        _dragOffset = e.GetCurrentPoint(_zoomBorder);

        e.Pointer.Capture(element);
        e.Handled = true;
    }

    private void MoveOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        IInputElement? element = (IInputElement?)sender;
        if (element == null || e.Pointer.Captured != element || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        e.Pointer.Capture(null);
        e.Handled = true;
    }

    private void MoveOnPointerMoved(object? sender, PointerEventArgs e)
    {
        IInputElement? element = (IInputElement?)sender;
        if (element == null || e.Pointer.Captured != element || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        e.Handled = true;
    }

    #endregion

    #region Resizing

    private void ResizeOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        IInputElement? element = (IInputElement?)sender;
        if (element == null || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        _dragOffset = e.GetCurrentPoint(_zoomBorder);

        e.Pointer.Capture(element);
        e.Handled = true;
    }

    private void ResizeOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        IInputElement? element = (IInputElement?)sender;
        if (element == null || e.Pointer.Captured != element || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        e.Pointer.Capture(null);
        e.Handled = true;
    }

    private void ResizeOnPointerMoved(object? sender, PointerEventArgs e)
    {
        IInputElement? element = (IInputElement?)sender;
        if (element == null || e.Pointer.Captured != element || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        e.Handled = true;
    }

    #endregion
}