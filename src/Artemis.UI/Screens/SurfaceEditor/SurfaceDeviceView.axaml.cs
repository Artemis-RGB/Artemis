using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using ReactiveUI.Avalonia;
using Avalonia.VisualTree;

namespace Artemis.UI.Screens.SurfaceEditor;

public partial class SurfaceDeviceView : ReactiveUserControl<SurfaceDeviceViewModel>
{
    private bool _dragging;

    public SurfaceDeviceView()
    {
        InitializeComponent();
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
                ViewModel.SurfaceEditorViewModel.UpdateSelection(new List<SurfaceDeviceViewModel> {ViewModel}, false, false);
                ViewModel.SurfaceEditorViewModel.FinishSelection();
            }

            ViewModel.SurfaceEditorViewModel.StartMouseDrag(point.Position);
            e.Pointer.Capture((IInputElement?) sender);
        }

        ViewModel.SurfaceEditorViewModel.UpdateMouseDrag(point.Position, e.KeyModifiers.HasFlag(KeyModifiers.Shift), e.KeyModifiers.HasFlag(KeyModifiers.Alt));

        e.Handled = true;
    }

    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (ViewModel == null || e.InitialPressMouseButton != MouseButton.Left)
            return;

        if (_dragging)
        {
            _dragging = false;
            ViewModel.SurfaceEditorViewModel.FinishSelection();
            e.Pointer.Capture(null);
        }
        else
        {
            ViewModel.SurfaceEditorViewModel.UpdateSelection(new List<SurfaceDeviceViewModel> {ViewModel}, e.KeyModifiers.HasFlag(KeyModifiers.Shift), e.KeyModifiers.HasFlag(KeyModifiers.Control));
            ViewModel.SurfaceEditorViewModel.FinishSelection();
        }

        e.Handled = true;
    }
}