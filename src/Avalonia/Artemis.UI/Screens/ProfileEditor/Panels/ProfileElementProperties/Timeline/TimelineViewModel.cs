using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.ProfileEditor;
using Avalonia.Controls.Mixins;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.ProfileElementProperties.Timeline;

public class TimelineViewModel : ActivatableViewModelBase
{
    private readonly IProfileEditorService _profileEditorService;
    private ObservableAsPropertyHelper<double>? _caretPosition;

    public TimelineViewModel(IProfileEditorService profileEditorService)
    {
        _profileEditorService = profileEditorService;
        this.WhenActivated(d =>
        {
            _caretPosition = _profileEditorService.Time
                .CombineLatest(_profileEditorService.PixelsPerSecond, (t, p) => t.TotalSeconds * p)
                .ToProperty(this, vm => vm.CaretPosition)
                .DisposeWith(d);
        });
    }

    public double CaretPosition => _caretPosition?.Value ?? 0.0;

    public void ChangeTime(TimeSpan newTime)
    {
        _profileEditorService.ChangeTime(newTime);
    }

    public TimeSpan SnapToTimeline(TimeSpan time, TimeSpan tolerance, bool snapToSegments, bool snapToCurrentTime, List<TimeSpan>? snapTimes = null)
    {
        return _profileEditorService.SnapToTimeline(time, tolerance, snapToSegments, snapToCurrentTime, snapTimes);
    }
}