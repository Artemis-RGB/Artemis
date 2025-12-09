using Avalonia.Input;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Profiles.ProfileEditor.Properties.Timeline.Segments;

public partial class MainSegmentView : ReactiveUserControl<MainSegmentViewModel>
{
    private double _dragOffset;

    public MainSegmentView()
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

    private void KeyframeDragStartAnchor_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
    }

    private void KeyframeDragStartAnchor_OnPointerMoved(object? sender, PointerEventArgs e)
    {
    }

    private void KeyframeDragStartAnchor_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
    }
}