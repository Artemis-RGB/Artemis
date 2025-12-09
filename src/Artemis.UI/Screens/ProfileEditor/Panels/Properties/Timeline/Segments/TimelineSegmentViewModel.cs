using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Screens.ProfileEditor.Properties.Dialogs;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Builders;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.ProfileEditor.Commands;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.Properties.Timeline.Segments;

public abstract partial class TimelineSegmentViewModel : ActivatableViewModelBase
{
    private static readonly TimeSpan NewSegmentLength = TimeSpan.FromSeconds(2);
    private readonly Dictionary<ILayerPropertyKeyframe, TimeSpan> _originalKeyframePositions = new();
    private readonly IProfileEditorService _profileEditorService;
    private readonly IWindowService _windowService;
    private TimeSpan _initialLength;
    private int _pixelsPerSecond;
    private RenderProfileElement? _profileElement;
    private ObservableAsPropertyHelper<bool>? _showAddEnd;
    private ObservableAsPropertyHelper<bool>? _showAddMain;
    private ObservableAsPropertyHelper<bool>? _showAddStart;
    [Notify] private ReactiveCommand<Unit, Unit>? _removeSegment;

    protected TimelineSegmentViewModel(IProfileEditorService profileEditorService, IWindowService windowService)
    {
        _profileEditorService = profileEditorService;
        _windowService = windowService;

        EditTime = ReactiveCommand.CreateFromTask(ExecuteEditTime);

        this.WhenActivated(d =>
        {
            profileEditorService.ProfileElement.Subscribe(p => _profileElement = p).DisposeWith(d);
            profileEditorService.PixelsPerSecond.Subscribe(p => _pixelsPerSecond = p).DisposeWith(d);

            _showAddStart = profileEditorService.ProfileElement
                .Select(p => p?.WhenAnyValue(element => element.Timeline.StartSegmentLength) ?? Observable.Never<TimeSpan>())
                .Switch()
                .Select(t => t == TimeSpan.Zero)
                .ToProperty(this, vm => vm.ShowAddStart)
                .DisposeWith(d);
            _showAddMain = profileEditorService.ProfileElement
                .Select(p => p?.WhenAnyValue(element => element.Timeline.MainSegmentLength) ?? Observable.Never<TimeSpan>())
                .Switch()
                .Select(t => t == TimeSpan.Zero)
                .ToProperty(this, vm => vm.ShowAddMain)
                .DisposeWith(d);
            _showAddEnd = profileEditorService.ProfileElement
                .Select(p => p?.WhenAnyValue(element => element.Timeline.EndSegmentLength) ?? Observable.Never<TimeSpan>())
                .Switch()
                .Select(t => t == TimeSpan.Zero)
                .ToProperty(this, vm => vm.ShowAddEnd)
                .DisposeWith(d);

            if (Type == ResizeTimelineSegment.SegmentType.Start)
                RemoveSegment = ReactiveCommand.Create(
                    ExecuteRemoveSegment,
                    this.WhenAnyValue(vm => vm.ShowAddMain).CombineLatest(this.WhenAnyValue(vm => vm.ShowAddEnd)).Select(tuple => !tuple.First || !tuple.Second)
                );

            if (Type == ResizeTimelineSegment.SegmentType.Main)
                RemoveSegment = ReactiveCommand.Create(
                    ExecuteRemoveSegment,
                    this.WhenAnyValue(vm => vm.ShowAddEnd).CombineLatest(this.WhenAnyValue(vm => vm.ShowAddStart)).Select(tuple => !tuple.First || !tuple.Second)
                );

            if (Type == ResizeTimelineSegment.SegmentType.End)
                RemoveSegment = ReactiveCommand.Create(
                    ExecuteRemoveSegment,
                    this.WhenAnyValue(vm => vm.ShowAddStart).CombineLatest(this.WhenAnyValue(vm => vm.ShowAddMain)).Select(tuple => !tuple.First || !tuple.Second)
                );
        });
    }

    public bool ShowAddStart => _showAddStart?.Value ?? false;
    public bool ShowAddMain => _showAddMain?.Value ?? false;
    public bool ShowAddEnd => _showAddEnd?.Value ?? false;

    public abstract TimeSpan Start { get; }
    public abstract double StartX { get; }
    public abstract TimeSpan End { get; }
    public abstract double EndX { get; }
    public abstract TimeSpan Length { get; set; }
    public abstract double Width { get; }
    public abstract string? EndTimestamp { get; }
    public abstract ResizeTimelineSegment.SegmentType Type { get; }

    public ReactiveCommand<Unit, Unit> EditTime { get; }

    public void AddStartSegment()
    {
        if (_profileElement == null)
            return;

        using ProfileEditorCommandScope scope = _profileEditorService.CreateCommandScope("Add start segment");
        ShiftKeyframes(_profileElement.GetAllLayerProperties().SelectMany(p => p.UntypedKeyframes), NewSegmentLength);
        ApplyPendingKeyframeMovement();
        _profileEditorService.ExecuteCommand(new ResizeTimelineSegment(ResizeTimelineSegment.SegmentType.Start, _profileElement, NewSegmentLength));
    }

    public void AddMainSegment()
    {
        if (_profileElement == null)
            return;

        using ProfileEditorCommandScope scope = _profileEditorService.CreateCommandScope("Add main segment");
        ShiftKeyframes(_profileElement.GetAllLayerProperties().SelectMany(p => p.UntypedKeyframes).Where(s => s.Position > _profileElement.Timeline.StartSegmentEndPosition), NewSegmentLength);
        ApplyPendingKeyframeMovement();
        _profileEditorService.ExecuteCommand(new ResizeTimelineSegment(ResizeTimelineSegment.SegmentType.Main, _profileElement, NewSegmentLength));
    }

    public void AddEndSegment()
    {
        if (_profileElement == null)
            return;

        using ProfileEditorCommandScope scope = _profileEditorService.CreateCommandScope("Add end segment");
        _profileEditorService.ExecuteCommand(new ResizeTimelineSegment(ResizeTimelineSegment.SegmentType.End, _profileElement, NewSegmentLength));
    }

    public void StartResize()
    {
        if (_profileElement == null)
            return;

        _initialLength = Length;
    }

    public void UpdateResize(double x, bool snap, bool round)
    {
        if (_profileElement == null)
            return;

        TimeSpan time = TimeSpan.FromMilliseconds(Math.Max(GetTimeFromX(x, snap, round).TotalMilliseconds, 100));
        TimeSpan difference = time - Length;
        List<ILayerPropertyKeyframe> keyframes = _profileElement.GetAllLayerProperties().SelectMany(p => p.UntypedKeyframes).ToList();
        ShiftKeyframes(keyframes.Where(k => k.Position > End.Add(difference)), difference);
        Length = time;
    }

    public void FinishResize(double x, bool snap, bool round)
    {
        if (_profileElement == null)
            return;

        TimeSpan time = TimeSpan.FromMilliseconds(Math.Max(GetTimeFromX(x, snap, round).TotalMilliseconds, 100));
        if (_initialLength == time)
            return;

        using ProfileEditorCommandScope scope = _profileEditorService.CreateCommandScope("Resize segment");
        ApplyPendingKeyframeMovement();
        _profileEditorService.ExecuteCommand(new ResizeTimelineSegment(Type, _profileElement, time, _initialLength));
    }

    protected TimeSpan GetTimeFromX(double x, bool snap, bool round)
    {
        TimeSpan time = TimeSpan.FromSeconds(x / _pixelsPerSecond);
        if (time < TimeSpan.Zero)
            time = TimeSpan.Zero;

        if (round)
            time = _profileEditorService.RoundTime(time);
        if (snap)
            time = SnapToTimeline(time);

        return time;
    }

    protected void ShiftKeyframes(IEnumerable<ILayerPropertyKeyframe> keyframes, TimeSpan amount)
    {
        foreach (ILayerPropertyKeyframe layerPropertyKeyframe in keyframes)
        {
            if (!_originalKeyframePositions.ContainsKey(layerPropertyKeyframe))
                _originalKeyframePositions[layerPropertyKeyframe] = layerPropertyKeyframe.Position;
            layerPropertyKeyframe.Position = layerPropertyKeyframe.Position.Add(amount);
        }
    }

    protected void ApplyPendingKeyframeMovement()
    {
        foreach ((ILayerPropertyKeyframe keyframe, TimeSpan originalPosition) in _originalKeyframePositions)
            _profileEditorService.ExecuteCommand(new MoveKeyframe(keyframe, keyframe.Position, originalPosition));

        _originalKeyframePositions.Clear();
    }

    private void ExecuteRemoveSegment()
    {
        if (_profileElement == null)
            return;

        using ProfileEditorCommandScope scope = _profileEditorService.CreateCommandScope("Remove segment");
        IEnumerable<ILayerPropertyKeyframe> keyframes = _profileElement.GetAllLayerProperties().SelectMany(p => p.UntypedKeyframes);

        // Delete keyframes in the segment
        foreach (ILayerPropertyKeyframe layerPropertyKeyframe in keyframes)
        {
            if (layerPropertyKeyframe.Position > Start && layerPropertyKeyframe.Position <= End)
                _profileEditorService.ExecuteCommand(new DeleteKeyframe(layerPropertyKeyframe));
        }

        // Move keyframes after the segment forwards
        ShiftKeyframes(keyframes.Where(s => s.Position > End), new TimeSpan(Length.Ticks * -1));
        ApplyPendingKeyframeMovement();

        _profileEditorService.ExecuteCommand(new ResizeTimelineSegment(Type, _profileElement, TimeSpan.Zero));
    }

    private async Task ExecuteEditTime()
    {
        await _windowService.CreateContentDialog()
            .WithTitle("Edit segment length")
            .WithViewModel(out TimelineSegmentEditViewModel vm, Length)
            .HavingPrimaryButton(b => b.WithText("Save").WithAction(() =>
            {
                if (_profileElement != null)
                    _profileEditorService.ExecuteCommand(new ResizeTimelineSegment(Type, _profileElement, TimeSpan.FromSeconds(vm.SegmentLength)));
            }))
            .WithDefaultButton(ContentDialogButton.Primary)
            .WithCloseButtonText("Cancel")
            .ShowAsync();
    }

    private TimeSpan SnapToTimeline(TimeSpan time)
    {
        TimeSpan tolerance = TimeSpan.FromMilliseconds(1000f / _pixelsPerSecond * 5);
        return _profileEditorService.SnapToTimeline(time, tolerance, false, true);
    }
}