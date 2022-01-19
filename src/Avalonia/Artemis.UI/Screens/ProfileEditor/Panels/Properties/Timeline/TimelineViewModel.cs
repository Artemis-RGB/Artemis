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

    public void SelectKeyframes(IEnumerable<ITimelineKeyframeViewModel> keyframes, bool expand)
    {
        _profileEditorService.SelectKeyframes(keyframes.Select(k => k.Keyframe), expand);
    }
}