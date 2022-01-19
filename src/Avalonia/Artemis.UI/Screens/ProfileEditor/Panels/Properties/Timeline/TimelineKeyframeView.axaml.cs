using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.Properties.Timeline;

public class TimelineKeyframeView : ReactiveUserControl<ITimelineKeyframeViewModel>
{
    private bool _moved;

    public TimelineKeyframeView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        e.Pointer.Capture((IInputElement?) sender);
        e.Handled = true;

        _moved = false;
    }

    private void InputElement_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (e.Pointer.Captured != sender)
            return;

        _moved = true;
    }

    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        e.Pointer.Capture(null);
        e.Handled = true;

        // Select the keyframe if the user didn't move
        if (!_moved)
            ViewModel?.Select(e.KeyModifiers.HasFlag(KeyModifiers.Shift), e.KeyModifiers.HasFlag(KeyModifiers.Control));
    }
}