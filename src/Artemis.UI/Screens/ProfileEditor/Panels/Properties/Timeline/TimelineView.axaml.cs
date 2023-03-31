using System.Collections.Generic;
using System.Linq;
using Artemis.UI.Screens.ProfileEditor.Properties.Timeline.Keyframes;
using Artemis.UI.Shared.Events;
using Artemis.UI.Shared.Extensions;
using Avalonia;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.VisualTree;

namespace Artemis.UI.Screens.ProfileEditor.Properties.Timeline;

public partial class TimelineView : ReactiveUserControl<TimelineViewModel>
{
    public TimelineView()
    {
        InitializeComponent();
    }


    private void SelectionRectangle_OnSelectionFinished(object? sender, SelectionRectangleEventArgs e)
    {
        if (ViewModel == null)
            return;

        List<TimelineKeyframeView> keyframeViews = this.GetVisualChildrenOfType<TimelineKeyframeView>().Where(k =>
        {
            TransformedBounds? transformedBounds = k.GetTransformedBounds();
            Rect hitTestRect = transformedBounds != null ? transformedBounds.Value.Bounds.TransformToAABB(transformedBounds.Value.Transform) : new Rect();
            return e.AbsoluteRectangle.Intersects(hitTestRect);
        }).ToList();

        ViewModel.SelectKeyframes(keyframeViews.Where(kv => kv.ViewModel != null).Select(kv => kv.ViewModel!), e.KeyModifiers.HasFlag(KeyModifiers.Shift));
    }

    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (SelectionRectangle.IsSelecting)
            return;

        ViewModel?.SelectKeyframes(new List<ITimelineKeyframeViewModel>(), false);
    }
}