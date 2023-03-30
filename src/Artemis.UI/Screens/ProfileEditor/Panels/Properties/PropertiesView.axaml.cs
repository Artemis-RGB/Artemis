using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.VisualTree;

namespace Artemis.UI.Screens.ProfileEditor.Properties;

public partial class PropertiesView : ReactiveUserControl<PropertiesViewModel>
{
    public PropertiesView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void ApplyTransition(bool enable)
    {
        // if (enable)
        // {
        //     ((DoubleTransition) _timelineCaret.Transitions![0]).Duration = TimeSpan.FromMilliseconds(50);
        //     ((DoubleTransition) _timelineLine.Transitions![0]).Duration = TimeSpan.FromMilliseconds(50);
        // }
        // else
        // {
        //     ((DoubleTransition) _timelineCaret.Transitions![0]).Duration = TimeSpan.Zero;
        //     ((DoubleTransition) _timelineLine.Transitions![0]).Duration = TimeSpan.Zero;
        // }
    }

    private void TimelineCaret_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        e.Pointer.Capture((IInputElement?) sender);
        ApplyTransition(false);
    }

    private void TimelineCaret_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        e.Pointer.Capture(null);
        ApplyTransition(true);
    }

    private void TimelineCaret_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed || ViewModel == null)
            return;

        Visual? senderElement = (Visual?) sender;
        if (senderElement == null)
            return;

        // Get the parent grid, need that for our position
        Visual? parent = senderElement.GetVisualParent();
        double x = Math.Max(0, e.GetPosition(parent).X);
        TimeSpan newTime = TimeSpan.FromSeconds(x / ViewModel.PixelsPerSecond);
        newTime = RoundTime(newTime);

        // If holding down shift, snap to the closest segment or keyframe
        if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            List<TimeSpan> snapTimes = ViewModel.PropertyGroupViewModels.SelectMany(g => g.GetAllKeyframeViewModels(true)).Select(k => k.Position).ToList();
            newTime = ViewModel.TimelineViewModel.SnapToTimeline(newTime, true, false, snapTimes);
        }

        // If holding down control, round to the closest 50ms
        if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
            newTime = TimeSpan.FromMilliseconds(Math.Round(newTime.TotalMilliseconds / 50.0) * 50.0);

        ViewModel.TimelineViewModel.ChangeTime(newTime);
    }

    private void TimelineHeader_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (ViewModel == null || sender is not Visual senderElement)
            return;

        // Get the parent grid, need that for our position
        double x = Math.Max(0, e.GetPosition(senderElement.GetVisualParent()).X);
        TimeSpan newTime = TimeSpan.FromSeconds(x / ViewModel.PixelsPerSecond);

        ViewModel.TimelineViewModel.ChangeTime(RoundTime(newTime));
    }

    private TimeSpan RoundTime(TimeSpan time)
    {
        // Round the time to something that fits the current zoom level
        if (ViewModel!.PixelsPerSecond < 200)
            return TimeSpan.FromMilliseconds(Math.Round(time.TotalMilliseconds / 5.0) * 5.0);
        if (ViewModel.PixelsPerSecond < 500)
            return TimeSpan.FromMilliseconds(Math.Round(time.TotalMilliseconds / 2.0) * 2.0);
        return TimeSpan.FromMilliseconds(Math.Round(time.TotalMilliseconds));
    }
}