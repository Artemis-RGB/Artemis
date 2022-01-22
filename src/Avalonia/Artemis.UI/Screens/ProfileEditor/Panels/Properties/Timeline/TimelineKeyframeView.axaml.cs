using System;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.Properties.Timeline;

public class TimelineKeyframeView : ReactiveUserControl<ITimelineKeyframeViewModel>
{
    private TimelinePropertyView? _timelinePropertyView;
    private TimelineView? _timelineView;
    private bool _moved;

    public TimelineKeyframeView()
    {
        InitializeComponent();
    }

    #region Overrides of TemplatedControl

    /// <inheritdoc />
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        _timelineView = this.FindLogicalAncestorOfType<TimelineView>();
        _timelinePropertyView = this.FindLogicalAncestorOfType<TimelinePropertyView>();

        base.OnAttachedToLogicalTree(e);
    }

    #endregion

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (ViewModel == null || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        e.Pointer.Capture((IInputElement?) sender);
        e.Handled = true;

        _moved = false;
    }

    private void InputElement_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (ViewModel == null || _timelineView?.ViewModel == null || e.Pointer.Captured != sender || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        if (!_moved)
            _timelineView.ViewModel.StartKeyframeMovement(ViewModel);
        _moved = true;

        TimeSpan time = ViewModel.GetTimeSpanAtPosition(e.GetPosition(_timelinePropertyView).X);
        _timelineView.ViewModel.UpdateKeyframeMovement(ViewModel, time, e.KeyModifiers.HasFlag(KeyModifiers.Shift), e.KeyModifiers.HasFlag(KeyModifiers.Control));
        e.Handled = true;
    }

    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (ViewModel == null || _timelineView?.ViewModel == null || e.Pointer.Captured != sender || e.InitialPressMouseButton != MouseButton.Left)
            return;

        e.Pointer.Capture(null);
        e.Handled = true;

        // Select the keyframe if the user didn't move
        if (!_moved)
            ViewModel.Select(e.KeyModifiers.HasFlag(KeyModifiers.Shift), e.KeyModifiers.HasFlag(KeyModifiers.Control));
        else
        {
            TimeSpan time = ViewModel.GetTimeSpanAtPosition(e.GetPosition(_timelinePropertyView).X);
            _timelineView.ViewModel.FinishKeyframeMovement(ViewModel, time, e.KeyModifiers.HasFlag(KeyModifiers.Shift), e.KeyModifiers.HasFlag(KeyModifiers.Control));
        }
    }

    private void FlyoutBase_OnOpening(object? sender, EventArgs e)
    {
        ViewModel?.PopulateEasingViewModels();
    }
}