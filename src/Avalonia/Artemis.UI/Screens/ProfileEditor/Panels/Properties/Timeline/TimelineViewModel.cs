using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.ProfileEditor;
using Avalonia.Controls.Mixins;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.Properties.Timeline;

public class TimelineViewModel : ActivatableViewModelBase
{
    private readonly IProfileEditorService _profileEditorService;
    private ObservableAsPropertyHelper<double>? _caretPosition;

    public TimelineViewModel(ObservableCollection<PropertyGroupViewModel> propertyGroupViewModels, IProfileEditorService profileEditorService)
    {
        PropertyGroupViewModels = propertyGroupViewModels;

        _profileEditorService = profileEditorService;
        this.WhenActivated(d =>
        {
            _caretPosition = _profileEditorService.Time
                .CombineLatest(_profileEditorService.PixelsPerSecond, (t, p) => t.TotalSeconds * p)
                .ToProperty(this, vm => vm.CaretPosition)
                .DisposeWith(d);
        });
    }

    public ObservableCollection<PropertyGroupViewModel> PropertyGroupViewModels { get; }

    public double CaretPosition => _caretPosition?.Value ?? 0.0;

    public void ChangeTime(TimeSpan newTime)
    {
        _profileEditorService.ChangeTime(newTime);
    }

    public TimeSpan SnapToTimeline(TimeSpan time, TimeSpan tolerance, bool snapToSegments, bool snapToCurrentTime, List<TimeSpan>? snapTimes = null)
    {
        return _profileEditorService.SnapToTimeline(time, tolerance, snapToSegments, snapToCurrentTime, snapTimes);
    }

    public void SelectKeyframes(List<ITimelineKeyframeViewModel> keyframes, bool expand)
    {
        List<ITimelineKeyframeViewModel> expandedKeyframes = PropertyGroupViewModels.SelectMany(g => g.GetAllKeyframeViewModels(true)).ToList();
        List<ITimelineKeyframeViewModel> collapsedKeyframes = PropertyGroupViewModels.SelectMany(g => g.GetAllKeyframeViewModels(false)).Except(expandedKeyframes).ToList();

        foreach (ITimelineKeyframeViewModel timelineKeyframeViewModel in collapsedKeyframes)
            timelineKeyframeViewModel.IsSelected = false;
        foreach (ITimelineKeyframeViewModel timelineKeyframeViewModel in expandedKeyframes)
        {
            if (timelineKeyframeViewModel.IsSelected && expand)
                continue;
            timelineKeyframeViewModel.IsSelected = keyframes.Contains(timelineKeyframeViewModel);
        }
    }

    public void SelectKeyframe(ITimelineKeyframeViewModel? clicked, bool selectBetween, bool toggle)
    {
        List<ITimelineKeyframeViewModel> expandedKeyframes = PropertyGroupViewModels.SelectMany(g => g.GetAllKeyframeViewModels(true)).ToList();
        List<ITimelineKeyframeViewModel> collapsedKeyframes = PropertyGroupViewModels.SelectMany(g => g.GetAllKeyframeViewModels(false)).Except(expandedKeyframes).ToList();

        foreach (ITimelineKeyframeViewModel timelineKeyframeViewModel in collapsedKeyframes)
            timelineKeyframeViewModel.IsSelected = false;

        if (clicked == null)
        {
            foreach (ITimelineKeyframeViewModel timelineKeyframeViewModel in expandedKeyframes)
                timelineKeyframeViewModel.IsSelected = false;

            return;
        }

        if (selectBetween)
        {
            int selectedIndex = expandedKeyframes.FindIndex(k => k.IsSelected);
            // If nothing is selected, select only the clicked
            if (selectedIndex == -1)
            {
                clicked.IsSelected = true;
                return;
            }

            foreach (ITimelineKeyframeViewModel keyframeViewModel in expandedKeyframes)
                keyframeViewModel.IsSelected = false;

            int clickedIndex = expandedKeyframes.IndexOf(clicked);
            if (clickedIndex < selectedIndex)
                foreach (ITimelineKeyframeViewModel keyframeViewModel in expandedKeyframes.Skip(clickedIndex).Take(selectedIndex - clickedIndex + 1))
                    keyframeViewModel.IsSelected = true;
            else
                foreach (ITimelineKeyframeViewModel keyframeViewModel in expandedKeyframes.Skip(selectedIndex).Take(clickedIndex - selectedIndex + 1))
                    keyframeViewModel.IsSelected = true;
        }
        else if (toggle)
        {
            // Toggle only the clicked keyframe, leave others alone
            clicked.IsSelected = !clicked.IsSelected;
        }
        else
        {
            // Only select the clicked keyframe
            foreach (ITimelineKeyframeViewModel keyframeViewModel in expandedKeyframes)
                keyframeViewModel.IsSelected = false;
            clicked.IsSelected = true;
        }
    }
}