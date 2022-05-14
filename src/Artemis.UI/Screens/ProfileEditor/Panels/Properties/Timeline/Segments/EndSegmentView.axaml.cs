using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.Properties.Timeline.Segments;

public class EndSegmentView : ReactiveUserControl<EndSegmentViewModel>
{
    private readonly Rectangle _keyframeDragAnchor;
    private double _dragOffset;

    public EndSegmentView()
    {
        InitializeComponent();
        _keyframeDragAnchor = this.Get<Rectangle>("KeyframeDragAnchor");
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void KeyframeDragAnchor_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (ViewModel == null || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;
        e.Pointer.Capture(_keyframeDragAnchor);

        _dragOffset = ViewModel.Width - e.GetCurrentPoint(this).Position.X;
        ViewModel.StartResize();
    }

    private void KeyframeDragAnchor_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (ViewModel == null || !ReferenceEquals(e.Pointer.Captured, _keyframeDragAnchor))
            return;
        ViewModel.UpdateResize(e.GetCurrentPoint(this).Position.X + _dragOffset);
    }

    private void KeyframeDragAnchor_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (ViewModel == null || !ReferenceEquals(e.Pointer.Captured, _keyframeDragAnchor))
            return;
        e.Pointer.Capture(null);
        ViewModel.FinishResize(e.GetCurrentPoint(this).Position.X + _dragOffset);
    }
}