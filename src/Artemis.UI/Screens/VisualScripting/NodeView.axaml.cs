using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using ReactiveUI.Avalonia;
using Avalonia.VisualTree;

namespace Artemis.UI.Screens.VisualScripting;

public partial class NodeView : ReactiveUserControl<NodeViewModel>
{
    private bool _dragging;

    public NodeView()
    {
        InitializeComponent();
    }

    #region Overrides of Layoutable

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize)
    {
        // Take the base implementation's size
        (double width, double height) = base.MeasureOverride(availableSize);

        // Ceil the resulting size   
        width = Math.Ceiling(width / 10.0) * 10.0;
        height = Math.Ceiling(height / 10.0) * 10.0;

        return new Size(width, height);
    }

    #endregion


    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (ViewModel == null || e.InitialPressMouseButton != MouseButton.Left)
            return;

        if (_dragging)
        {
            _dragging = false;
            ViewModel.NodeScriptViewModel.FinishNodeDrag();
            e.Pointer.Capture(null);
        }
        else
        {
            ViewModel.NodeScriptViewModel.UpdateNodeSelection(new List<NodeViewModel> {ViewModel}, e.KeyModifiers.HasFlag(KeyModifiers.Shift), e.KeyModifiers.HasFlag(KeyModifiers.Control));
            ViewModel.NodeScriptViewModel.FinishNodeSelection();
        }

        e.Handled = true;
    }

    private void InputElement_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        PointerPoint point = e.GetCurrentPoint(this.FindAncestorOfType<Canvas>());
        if (ViewModel == null || !point.Properties.IsLeftButtonPressed)
            return;

        if (!_dragging)
        {
            _dragging = true;

            if (!ViewModel.IsSelected)
            {
                ViewModel.NodeScriptViewModel.UpdateNodeSelection(new List<NodeViewModel> {ViewModel}, false, false);
                ViewModel.NodeScriptViewModel.FinishNodeSelection();
            }

            ViewModel.NodeScriptViewModel.StartNodeDrag(point.Position);
            e.Pointer.Capture((IInputElement?) sender);
        }

        ViewModel.NodeScriptViewModel.UpdateNodeDrag(point.Position);

        e.Handled = true;
    }

    private void NodeContainer_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        e.Handled = true;
    }
}