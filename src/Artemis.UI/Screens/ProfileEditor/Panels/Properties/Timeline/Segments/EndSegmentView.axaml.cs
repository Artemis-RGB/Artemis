using Avalonia.Input;
using ReactiveUI.Avalonia;

namespace Artemis.UI.Screens.ProfileEditor.Properties.Timeline.Segments;

public partial class EndSegmentView : ReactiveUserControl<EndSegmentViewModel>
{
    private double _dragOffset;

    public EndSegmentView()
    {
        InitializeComponent();
    }


    private void KeyframeDragAnchor_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (ViewModel == null || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;
        e.Pointer.Capture(KeyframeDragAnchor);

        _dragOffset = ViewModel.Width - e.GetCurrentPoint(this).Position.X;
        ViewModel.StartResize();
    }

    private void KeyframeDragAnchor_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (ViewModel == null || !ReferenceEquals(e.Pointer.Captured, KeyframeDragAnchor))
            return;
        ViewModel.UpdateResize(e.GetCurrentPoint(this).Position.X + _dragOffset, e.KeyModifiers.HasFlag(KeyModifiers.Shift), e.KeyModifiers.HasFlag(KeyModifiers.Control));
    }

    private void KeyframeDragAnchor_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (ViewModel == null || !ReferenceEquals(e.Pointer.Captured, KeyframeDragAnchor))
            return;
        e.Pointer.Capture(null);
        ViewModel.FinishResize(e.GetCurrentPoint(this).Position.X + _dragOffset, e.KeyModifiers.HasFlag(KeyModifiers.Shift), e.KeyModifiers.HasFlag(KeyModifiers.Control));
    }
}