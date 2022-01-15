using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.ProfileEditor;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.Playback;

public class PlaybackViewModel : ActivatableViewModelBase
{
    private readonly ICoreService _coreService;
    private readonly IProfileEditorService _profileEditorService;
    private readonly ISettingsService _settingsService;
    private RenderProfileElement? _profileElement;
    private ObservableAsPropertyHelper<TimeSpan>? _currentTime;
    private ObservableAsPropertyHelper<string?>? _formattedCurrentTime;
    private ObservableAsPropertyHelper<bool>? _playing;
    private bool _repeating;
    private bool _repeatTimeline;
    private bool _repeatSegment;

    public PlaybackViewModel(ICoreService coreService, IProfileEditorService profileEditorService, ISettingsService settingsService)
    {
        _coreService = coreService;
        _profileEditorService = profileEditorService;
        _settingsService = settingsService;

        this.WhenActivated(d =>
        {
            _profileEditorService.ProfileElement.Subscribe(e => _profileElement = e).DisposeWith(d);
            _currentTime = _profileEditorService.Time.ToProperty(this, vm => vm.CurrentTime).DisposeWith(d);
            _formattedCurrentTime = _profileEditorService.Time.Select(t => $"{Math.Floor(t.TotalSeconds):00}.{t.Milliseconds:000}").ToProperty(this, vm => vm.FormattedCurrentTime).DisposeWith(d);
            _playing = _profileEditorService.Playing.ToProperty(this, vm => vm.Playing).DisposeWith(d);

            Observable.FromEventPattern<FrameRenderingEventArgs>(x => coreService.FrameRendering += x, x => coreService.FrameRendering -= x)
                .Subscribe(e => CoreServiceOnFrameRendering(e.EventArgs))
                .DisposeWith(d);
        });
    }

    public TimeSpan CurrentTime => _currentTime?.Value ?? TimeSpan.Zero;
    public string? FormattedCurrentTime => _formattedCurrentTime?.Value;
    public bool Playing => _playing?.Value ?? false;

    public bool Repeating
    {
        get => _repeating;
        set => this.RaiseAndSetIfChanged(ref _repeating, value);
    }

    public bool RepeatTimeline
    {
        get => _repeatTimeline;
        set => this.RaiseAndSetIfChanged(ref _repeatTimeline, value);
    }

    public bool RepeatSegment
    {
        get => _repeatSegment;
        set => this.RaiseAndSetIfChanged(ref _repeatSegment, value);
    }

    public void PlayFromStart()
    {
        GoToStart();
        if (!Playing)
            _profileEditorService.Play();
    }

    public void TogglePlay()
    {
        if (!Playing)
            _profileEditorService.Play();
        else
            _profileEditorService.Pause();
    }

    public void GoToStart()
    {
        _profileEditorService.ChangeTime(TimeSpan.Zero);
    }

    public void GoToEnd()
    {
        if (_profileElement == null)
            return;

        _profileEditorService.ChangeTime(_profileElement.Timeline.EndSegmentEndPosition);
    }

    public void GoToPreviousFrame()
    {
        if (_profileElement == null)
            return;

        double frameTime = 1000.0 / _settingsService.GetSetting("Core.TargetFrameRate", 30).Value;
        double newTime = Math.Max(0, Math.Round((CurrentTime.TotalMilliseconds - frameTime) / frameTime) * frameTime);
        _profileEditorService.ChangeTime(TimeSpan.FromMilliseconds(newTime));
    }

    public void GoToNextFrame()
    {
        if (_profileElement == null)
            return;

        double frameTime = 1000.0 / _settingsService.GetSetting("Core.TargetFrameRate", 30).Value;
        double newTime = Math.Round((CurrentTime.TotalMilliseconds + frameTime) / frameTime) * frameTime;
        newTime = Math.Min(newTime, _profileElement.Timeline.EndSegmentEndPosition.TotalMilliseconds);
        _profileEditorService.ChangeTime(TimeSpan.FromMilliseconds(newTime));
    }

    public void CycleRepeating()
    {
        if (!Repeating)
        {
            RepeatTimeline = true;
            RepeatSegment = false;
            Repeating = true;
        }
        else if (RepeatTimeline)
        {
            RepeatTimeline = false;
            RepeatSegment = true;
        }
        else if (RepeatSegment)
        {
            RepeatTimeline = true;
            RepeatSegment = false;
            Repeating = false;
        }
    }

    private TimeSpan GetCurrentSegmentStart()
    {
        if (_profileElement == null)
            return TimeSpan.Zero;

        if (CurrentTime < _profileElement.Timeline.StartSegmentEndPosition)
            return TimeSpan.Zero;
        if (CurrentTime < _profileElement.Timeline.MainSegmentEndPosition)
            return _profileElement.Timeline.MainSegmentStartPosition;
        if (CurrentTime < _profileElement.Timeline.EndSegmentEndPosition)
            return _profileElement.Timeline.EndSegmentStartPosition;

        return TimeSpan.Zero;
    }

    private TimeSpan GetCurrentSegmentEnd()
    {
        if (_profileElement == null)
            return TimeSpan.Zero;

        if (CurrentTime < _profileElement.Timeline.StartSegmentEndPosition)
            return _profileElement.Timeline.StartSegmentEndPosition;
        if (CurrentTime < _profileElement.Timeline.MainSegmentEndPosition)
            return _profileElement.Timeline.MainSegmentEndPosition;
        if (CurrentTime < _profileElement.Timeline.EndSegmentEndPosition)
            return _profileElement.Timeline.EndSegmentEndPosition;

        return TimeSpan.Zero;
    }

    private void CoreServiceOnFrameRendering(FrameRenderingEventArgs e)
    {
        if (!Playing)
            return;

        TimeSpan newTime = CurrentTime.Add(TimeSpan.FromSeconds(e.DeltaTime));
        if (_profileElement != null)
        {
            if (Repeating && RepeatTimeline)
            {
                if (newTime > _profileElement.Timeline.Length)
                    newTime = TimeSpan.Zero;
            }
            else if (Repeating && RepeatSegment)
            {
                if (newTime > GetCurrentSegmentEnd())
                    newTime = GetCurrentSegmentStart();
            }
            else if (newTime > _profileElement.Timeline.Length)
            {
                newTime = _profileElement.Timeline.Length;
                _profileEditorService.Pause();
            }
        }

        _profileEditorService.ChangeTime(newTime);
    }
}