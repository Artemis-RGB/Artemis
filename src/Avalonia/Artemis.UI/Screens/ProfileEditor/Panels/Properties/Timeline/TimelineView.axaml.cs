using System.Collections.Generic;
using System.Linq;
using Artemis.UI.Shared.Controls;
using Artemis.UI.Shared.Events;
using Artemis.UI.Shared.Extensions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.Properties.Timeline;

public class TimelineView : ReactiveUserControl<TimelineViewModel>
{
    private readonly SelectionRectangle _selectionRectangle;

    public TimelineView()
    {
        InitializeComponent();
        _selectionRectangle = this.Get<SelectionRectangle>("SelectionRectangle");
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

        ViewModel.SelectKeyframes(keyframeViews.Where(kv => kv.ViewModel != null).Select(kv => kv.ViewModel!), e.KeyModifiers.HasFlag(KeyModifiers.Shift));
    }

    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_selectionRectangle.IsSelecting)
            return;

        ViewModel?.SelectKeyframes(new List<ITimelineKeyframeViewModel>(), false);
    }
}