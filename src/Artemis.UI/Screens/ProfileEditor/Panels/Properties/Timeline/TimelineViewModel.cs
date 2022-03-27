using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.Properties.Timeline.Keyframes;
using Artemis.UI.Screens.ProfileEditor.Properties.Timeline.Segments;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.ProfileEditor.Commands;
using Avalonia.Controls.Mixins;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.Properties.Timeline;

public class TimelineViewModel : ActivatableViewModelBase
{
    private readonly IProfileEditorService _profileEditorService;
    private ObservableAsPropertyHelper<double>? _caretPosition;
    private ObservableAsPropertyHelper<int>? _pixelsPerSecond;
    private List<ITimelineKeyframeViewModel>? _moveKeyframes;
    private ObservableAsPropertyHelper<double> _minWidth;

    public TimelineViewModel(ObservableCollection<PropertyGroupViewModel> propertyGroupViewModels,
        StartSegmentViewModel startSegmentViewModel,
        MainSegmentViewModel mainSegmentViewModel, 
        EndSegmentViewModel endSegmentViewModel,
        IProfileEditorService profileEditorService)
    {
        PropertyGroupViewModels = propertyGroupViewModels;
        StartSegmentViewModel = startSegmentViewModel;
        MainSegmentViewModel = mainSegmentViewModel;
        EndSegmentViewModel = endSegmentViewModel;

        _profileEditorService = profileEditorService;
        this.WhenActivated(d =>
        {
            _caretPosition = _profileEditorService.Time
                .CombineLatest(_profileEditorService.PixelsPerSecond, (t, p) => t.TotalSeconds * p)
                .ToProperty(this, vm => vm.CaretPosition)
                .DisposeWith(d);
            _pixelsPerSecond = _profileEditorService.PixelsPerSecond.ToProperty(this, vm => vm.PixelsPerSecond).DisposeWith(d);
            _minWidth = profileEditorService.ProfileElement
                .Select(p => p?.WhenAnyValue(element => element.Timeline.Length) ?? Observable.Never<TimeSpan>())
                .Switch()
                .CombineLatest(profileEditorService.PixelsPerSecond, (t, p) => t.TotalSeconds * p + 100)
                .ToProperty(this, vm => vm.MinWidth)
                .DisposeWith(d);
        });
    }

    public ObservableCollection<PropertyGroupViewModel> PropertyGroupViewModels { get; }
    public StartSegmentViewModel StartSegmentViewModel { get; }
    public MainSegmentViewModel MainSegmentViewModel { get; }
    public EndSegmentViewModel EndSegmentViewModel { get; }

    public double CaretPosition => _caretPosition?.Value ?? 0.0;
    public int PixelsPerSecond => _pixelsPerSecond?.Value ?? 0;
    public double MinWidth => _minWidth?.Value ?? 0;

    public void ChangeTime(TimeSpan newTime)
    {
        _profileEditorService.ChangeTime(newTime);
    }

    public TimeSpan SnapToTimeline(TimeSpan time, bool snapToSegments, bool snapToCurrentTime, List<TimeSpan>? snapTimes = null)
    {
        TimeSpan tolerance = TimeSpan.FromMilliseconds(1000f / PixelsPerSecond * 5);
        return _profileEditorService.SnapToTimeline(time, tolerance, snapToSegments, snapToCurrentTime, snapTimes);
    }

    public void SelectKeyframes(IEnumerable<ITimelineKeyframeViewModel> keyframes, bool expand)
    {
        _profileEditorService.SelectKeyframes(keyframes.Select(k => k.Keyframe), expand);
    }

    #region Keyframe movement

    public void StartKeyframeMovement(ITimelineKeyframeViewModel source)
    {
        if (!source.IsSelected)
            SelectKeyframes(new List<ITimelineKeyframeViewModel> {source}, false);

        _moveKeyframes = PropertyGroupViewModels.SelectMany(g => g.GetAllKeyframeViewModels(true)).Where(k => k.IsSelected).ToList();

        source.StartMovement(source);
        foreach (ITimelineKeyframeViewModel keyframeViewModel in _moveKeyframes)
            keyframeViewModel.StartMovement(source);
    }

    public void UpdateKeyframeMovement(ITimelineKeyframeViewModel source, TimeSpan time, bool snap, bool round)
    {
        if (_moveKeyframes == null)
            return;

        if (round)
            time = _profileEditorService.RoundTime(time);
        if (snap)
            time = SnapToTimeline(time, true, true);

        // Always update source first
        source.UpdateMovement(time);
        foreach (ITimelineKeyframeViewModel keyframeViewModel in _moveKeyframes)
            keyframeViewModel.UpdateMovement(time);
    }

    public void FinishKeyframeMovement(ITimelineKeyframeViewModel source, TimeSpan time, bool snap, bool round)
    {
        if (_moveKeyframes == null)
            return;

        if (round)
            time = _profileEditorService.RoundTime(time);
        if (snap)
            time = SnapToTimeline(time, true, true);

        // If only one selected it's always the source, update it as a single command
        if (_moveKeyframes.Count == 1)
        {
            source.UpdateMovement(time);
            source.FinishMovement();
        }
        // Otherwise update in a scope
        else
        {
            using ProfileEditorCommandScope scope = _profileEditorService.CreateCommandScope($"Move {_moveKeyframes.Count} keyframes.");
            // Always update source first
            source.UpdateMovement(time);
            foreach (ITimelineKeyframeViewModel keyframeViewModel in _moveKeyframes)
            {
                keyframeViewModel.UpdateMovement(time);
                keyframeViewModel.FinishMovement();
            }
        }
    }

    #endregion

    #region Keyframe actions

    public void DuplicateKeyframes(ITimelineKeyframeViewModel? source = null)
    {
        if (source is { IsSelected: false })
            source.Delete();
        else
        {
            List<ITimelineKeyframeViewModel> keyframes = PropertyGroupViewModels.SelectMany(g => g.GetAllKeyframeViewModels(true)).Where(k => k.IsSelected).ToList();
            using ProfileEditorCommandScope scope = _profileEditorService.CreateCommandScope($"Duplicate {keyframes.Count} keyframes.");
            foreach (ITimelineKeyframeViewModel timelineKeyframeViewModel in keyframes)
                timelineKeyframeViewModel.Duplicate();
        }
    }

    public void CopyKeyframes(ITimelineKeyframeViewModel? source = null)
    {
        if (source is { IsSelected: false })
            source.Copy();
        else
        {
            List<ITimelineKeyframeViewModel> keyframes = PropertyGroupViewModels.SelectMany(g => g.GetAllKeyframeViewModels(true)).Where(k => k.IsSelected).ToList();
            using ProfileEditorCommandScope scope = _profileEditorService.CreateCommandScope($"Copy {keyframes.Count} keyframes.");
            foreach (ITimelineKeyframeViewModel timelineKeyframeViewModel in keyframes)
                timelineKeyframeViewModel.Copy();
        }
    }

    public void PasteKeyframes(ITimelineKeyframeViewModel? source = null)
    {
        if (source is { IsSelected: false })
            source.Paste();
        else
        {
            List<ITimelineKeyframeViewModel> keyframes = PropertyGroupViewModels.SelectMany(g => g.GetAllKeyframeViewModels(true)).Where(k => k.IsSelected).ToList();
            using ProfileEditorCommandScope scope = _profileEditorService.CreateCommandScope($"Paste {keyframes.Count} keyframes.");
            foreach (ITimelineKeyframeViewModel timelineKeyframeViewModel in keyframes)
                timelineKeyframeViewModel.Paste();
        }
    }

    public void DeleteKeyframes(ITimelineKeyframeViewModel? source = null)
    {
        if (source is {IsSelected: false})
            source.Delete();
        else
        {
            List<ITimelineKeyframeViewModel> keyframes = PropertyGroupViewModels.SelectMany(g => g.GetAllKeyframeViewModels(true)).Where(k => k.IsSelected).ToList();
            using ProfileEditorCommandScope scope = _profileEditorService.CreateCommandScope($"Delete {keyframes.Count} keyframes.");
            foreach (ITimelineKeyframeViewModel timelineKeyframeViewModel in keyframes)
                timelineKeyframeViewModel.Delete();
        }
    }

    #endregion
}