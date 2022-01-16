using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.VisualTree;

namespace Artemis.UI.Screens.ProfileEditor.ProfileElementProperties;

public class ProfileElementPropertiesView : ReactiveUserControl<ProfileElementPropertiesViewModel>
{
    private Polygon _timelineCaret;
    private Line _timelineLine;

    public ProfileElementPropertiesView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        _timelineCaret = this.Get<Polygon>("TimelineCaret");
        _timelineLine = this.Get<Line>("TimelineLine");
    }

    private void ApplyTransition(bool enable)
    {
        if (enable)
        {
            ((DoubleTransition) _timelineCaret.Transitions![0]).Duration = TimeSpan.FromMilliseconds(50);
            ((DoubleTransition) _timelineLine.Transitions![0]).Duration = TimeSpan.FromMilliseconds(50);
        }
        else
        {
            ((DoubleTransition) _timelineCaret.Transitions![0]).Duration = TimeSpan.Zero;
            ((DoubleTransition) _timelineLine.Transitions![0]).Duration = TimeSpan.Zero;
        }
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

        IInputElement? senderElement = (IInputElement?) sender;
        if (senderElement == null)
            return;

        // Get the parent grid, need that for our position
        IVisual? parent = senderElement.VisualParent;
        double x = Math.Max(0, e.GetPosition(parent).X);
        TimeSpan newTime = TimeSpan.FromSeconds(x / ViewModel.PixelsPerSecond);

        // Round the time to something that fits the current zoom level
        if (ViewModel.PixelsPerSecond < 200)
            newTime = TimeSpan.FromMilliseconds(Math.Round(newTime.TotalMilliseconds / 5.0) * 5.0);
        else if (ViewModel.PixelsPerSecond < 500)
            newTime = TimeSpan.FromMilliseconds(Math.Round(newTime.TotalMilliseconds / 2.0) * 2.0);
        else
            newTime = TimeSpan.FromMilliseconds(Math.Round(newTime.TotalMilliseconds));

        // If holding down shift, snap to the closest segment or keyframe
        if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            List<TimeSpan> snapTimes = ViewModel.PropertyGroupViewModels.SelectMany(g => g.GetAllKeyframeViewModels(true)).Select(k => k.Position).ToList();
            newTime = ViewModel.TimelineViewModel.SnapToTimeline(newTime, TimeSpan.FromMilliseconds(1000f / ViewModel.PixelsPerSecond * 5), true, false, snapTimes);
        }

        // If holding down control, round to the closest 50ms
        if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
            newTime = TimeSpan.FromMilliseconds(Math.Round(newTime.TotalMilliseconds / 50.0) * 50.0);

        ViewModel.TimelineViewModel.ChangeTime(newTime);
    }
}