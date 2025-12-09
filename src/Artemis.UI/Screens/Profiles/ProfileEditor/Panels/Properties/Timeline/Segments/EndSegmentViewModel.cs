using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.ProfileEditor.Commands;
using ReactiveUI;

namespace Artemis.UI.Screens.Profiles.ProfileEditor.Properties.Timeline.Segments;

public class EndSegmentViewModel : TimelineSegmentViewModel
{
    private readonly ObservableAsPropertyHelper<double> _width;
    private ObservableAsPropertyHelper<double>? _end;
    private ObservableAsPropertyHelper<string?>? _endTimestamp;
    private RenderProfileElement? _profileElement;
    private ObservableAsPropertyHelper<double>? _start;

    public EndSegmentViewModel(IProfileEditorService profileEditorService, IWindowService windowService) : base(profileEditorService, windowService)
    {
        this.WhenActivated(d =>
        {
            profileEditorService.ProfileElement.Subscribe(p => _profileElement = p).DisposeWith(d);

            _start = profileEditorService.ProfileElement
                .Select(p => p?.WhenAnyValue(element => element.Timeline.EndSegmentStartPosition) ?? Observable.Never<TimeSpan>())
                .Switch()
                .CombineLatest(profileEditorService.PixelsPerSecond, (t, p) => t.TotalSeconds * p)
                .ToProperty(this, vm => vm.StartX)
                .DisposeWith(d);
            _end = profileEditorService.ProfileElement
                .Select(p => p?.WhenAnyValue(element => element.Timeline.EndSegmentEndPosition) ?? Observable.Never<TimeSpan>())
                .Switch()
                .CombineLatest(profileEditorService.PixelsPerSecond, (t, p) => t.TotalSeconds * p)
                .ToProperty(this, vm => vm.EndX)
                .DisposeWith(d);
            _endTimestamp = profileEditorService.ProfileElement
                .Select(p => p?.WhenAnyValue(element => element.Timeline.EndSegmentEndPosition) ?? Observable.Never<TimeSpan>())
                .Switch()
                .Select(p => $"{Math.Floor(p.TotalSeconds):00}.{p.Milliseconds:000}")
                .ToProperty(this, vm => vm.EndTimestamp)
                .DisposeWith(d);
        });

        _width = this.WhenAnyValue(vm => vm.StartX, vm => vm.EndX).Select(t => t.Item2 - t.Item1).ToProperty(this, vm => vm.Width);
    }

    public override TimeSpan Start => _profileElement?.Timeline.EndSegmentStartPosition ?? TimeSpan.Zero;
    public override double StartX => _start?.Value ?? 0;
    public override TimeSpan End => _profileElement?.Timeline.EndSegmentEndPosition ?? TimeSpan.Zero;
    public override double EndX => _end?.Value ?? 0;

    public override TimeSpan Length
    {
        get => _profileElement?.Timeline.EndSegmentLength ?? TimeSpan.Zero;
        set
        {
            if (_profileElement != null)
                _profileElement.Timeline.EndSegmentLength = value;
            this.RaisePropertyChanged(nameof(Length));
        }
    }

    public override double Width => _width.Value;

    public override string? EndTimestamp => _endTimestamp?.Value;
    public override ResizeTimelineSegment.SegmentType Type => ResizeTimelineSegment.SegmentType.End;
}