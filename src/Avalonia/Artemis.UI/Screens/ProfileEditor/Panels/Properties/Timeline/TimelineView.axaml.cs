using System.Collections.Generic;
using System.Linq;
using Artemis.UI.Shared.Events;
using Artemis.UI.Shared.Extensions;
using Avalonia;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.Properties.Timeline;

public class TimelineView : ReactiveUserControl<TimelineViewModel>
{
    private bool _draggedCursor;

    public TimelineView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void SelectionRectangle_OnSelectionFinished(object? sender, SelectionRectangleEventArgs e)
    {
        if (ViewModel == null)
            return;

        List<TimelineKeyframeView> keyframeViews = this.GetVisualChildrenOfType<TimelineKeyframeView>().Where(k =>
        {
            Rect hitTestRect = k.TransformedBounds != null ? k.TransformedBounds.Value.Bounds.TransformToAABB(k.TransformedBounds.Value.Transform) : Rect.Empty;
            return e.AbsoluteRectangle.Intersects(hitTestRect);
        }).ToList();

        ViewModel.SelectKeyframes(keyframeViews.Where(kv => kv.ViewModel != null).Select(kv => kv.ViewModel!).ToList(), e.KeyModifiers.HasFlag(KeyModifiers.Shift));
    }

    private void InputElement_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_draggedCursor)
            return;

        _draggedCursor = e.GetCurrentPoint(this).Properties.IsLeftButtonPressed;
    }

    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (ViewModel == null)
            return;

        if (_draggedCursor)
        {
            _draggedCursor = false;
            return;
        }

        Point position = e.GetPosition(VisualRoot);
        TimelineKeyframeView? keyframeView = this.GetVisualChildrenOfType<TimelineKeyframeView>().Where(k =>
        {
            Rect hitTestRect = k.TransformedBounds != null ? k.TransformedBounds.Value.Bounds.TransformToAABB(k.TransformedBounds.Value.Transform) : Rect.Empty;
            return hitTestRect.Contains(position);
        }).FirstOrDefault(kv => kv.ViewModel != null);

        ViewModel.SelectKeyframe(keyframeView?.ViewModel, e.KeyModifiers.HasFlag(KeyModifiers.Shift), false);
    }
}