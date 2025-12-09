using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Models;
using Artemis.UI.Screens.Profiles.ProfileEditor.Properties.Timeline.Keyframes;
using Artemis.UI.Screens.Profiles.ProfileEditor.Properties.Timeline.Segments;
using Artemis.UI.Services.ProfileEditor.Commands;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Extensions;
using Artemis.UI.Shared.Services.ProfileEditor;
using ReactiveUI;

namespace Artemis.UI.Screens.Profiles.ProfileEditor.Properties.Timeline;

public class TimelineViewModel : ActivatableViewModelBase
{
    private readonly IProfileEditorService _profileEditorService;
    private ObservableAsPropertyHelper<double>? _caretPosition;
    private ObservableAsPropertyHelper<double>? _minWidth;
    private List<ITimelineKeyframeViewModel>? _moveKeyframes;
    private ObservableAsPropertyHelper<int>? _pixelsPerSecond;
    private RenderProfileElement? _profileElement;
    private TimeSpan _time;

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
            _profileEditorService.ProfileElement.Subscribe(p => _profileElement = p).DisposeWith(d);
            _profileEditorService.Time.Subscribe(t => _time = t).DisposeWith(d);
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

        DuplicateSelectedKeyframes = ReactiveCommand.Create(ExecuteDuplicateSelectedKeyframes);
        CopySelectedKeyframes = ReactiveCommand.Create(ExecuteCopySelectedKeyframes);
        PasteKeyframes = ReactiveCommand.CreateFromTask(ExecutePasteKeyframes);
        DeleteSelectedKeyframes = ReactiveCommand.Create(ExecuteDeleteSelectedKeyframes);
    }

    public ObservableCollection<PropertyGroupViewModel> PropertyGroupViewModels { get; }
    public StartSegmentViewModel StartSegmentViewModel { get; }
    public MainSegmentViewModel MainSegmentViewModel { get; }
    public EndSegmentViewModel EndSegmentViewModel { get; }
    public ReactiveCommand<Unit, Unit> DuplicateSelectedKeyframes { get; }
    public ReactiveCommand<Unit, Unit> CopySelectedKeyframes { get; }
    public ReactiveCommand<Unit, Unit> PasteKeyframes { get; }
    public ReactiveCommand<Unit, Unit> DeleteSelectedKeyframes { get; }

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

    private void ExecuteDuplicateSelectedKeyframes()
    {
        PropertyGroupViewModels.SelectMany(g => g.GetAllKeyframeViewModels(true)).FirstOrDefault(k => k.IsSelected)?.Duplicate.Execute().Subscribe();
    }

    private void ExecuteCopySelectedKeyframes()
    {
        PropertyGroupViewModels.SelectMany(g => g.GetAllKeyframeViewModels(true)).FirstOrDefault(k => k.IsSelected)?.Copy.Execute().Subscribe();
    }

    private async Task ExecutePasteKeyframes()
    {
        if (_profileElement == null)
            return;

        List<KeyframeClipboardModel>? keyframes = await Shared.UI.Clipboard.GetJsonAsync<List<KeyframeClipboardModel>>(KeyframeClipboardModel.ClipboardDataFormat);
        if (keyframes == null)
            return;

        PasteKeyframes command = new(_profileElement, keyframes, _time);
        _profileEditorService.ExecuteCommand(command);
        if (command.PastedKeyframes != null && command.PastedKeyframes.Any())
            _profileEditorService.SelectKeyframes(command.PastedKeyframes, false);
    }

    private void ExecuteDeleteSelectedKeyframes()
    {
        PropertyGroupViewModels.SelectMany(g => g.GetAllKeyframeViewModels(true)).FirstOrDefault(k => k.IsSelected)?.Delete.Execute().Subscribe();
    }

    #endregion
}