﻿using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Timers;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.ProfileEditor;
using Avalonia.Threading;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.Profiles.ProfileEditor.Playback;

public partial class PlaybackViewModel : ActivatableViewModelBase
{
    private readonly IProfileEditorService _profileEditorService;
    private readonly ISettingsService _settingsService;
    private ObservableAsPropertyHelper<TimeSpan>? _currentTime;
    private ObservableAsPropertyHelper<string?>? _formattedCurrentTime;
    private ObservableAsPropertyHelper<bool>? _keyBindingsEnabled;
    private DateTime _lastUpdate;
    private ObservableAsPropertyHelper<bool>? _playing;
    private RenderProfileElement? _profileElement;
    [Notify] private bool _repeating;
    [Notify] private bool _repeatSegment;
    [Notify] private bool _repeatTimeline;

    public PlaybackViewModel(IProfileEditorService profileEditorService, ISettingsService settingsService)
    {
        _profileEditorService = profileEditorService;
        _settingsService = settingsService;

        if (_settingsService.GetSetting("ProfileEditor.RepeatTimeline", true).Value)
        {
            _repeating = true;
            _repeatTimeline = true;
        }
        else if (_settingsService.GetSetting("ProfileEditor.RepeatSegment", false).Value)
        {
            _repeating = true;
            _repeatSegment = true;
        }
        else
        {
            _repeating = false;
            _repeatTimeline = true;
        }

        this.WhenActivated(d =>
        {
            _profileEditorService.ProfileElement.Subscribe(e => _profileElement = e).DisposeWith(d);
            _currentTime = _profileEditorService.Time.ToProperty(this, vm => vm.CurrentTime).DisposeWith(d);
            _formattedCurrentTime = _profileEditorService.Time.Select(t => $"{Math.Floor(t.TotalSeconds):00}.{t.Milliseconds:000}").ToProperty(this, vm => vm.FormattedCurrentTime).DisposeWith(d);
            _playing = _profileEditorService.Playing.ToProperty(this, vm => vm.Playing).DisposeWith(d);
            _keyBindingsEnabled = Shared.UI.CurrentKeyBindingsEnabled.ToProperty(this, vm => vm.KeyBindingsEnabled).DisposeWith(d);
            
            // Update timer
            Timer updateTimer = new(TimeSpan.FromMilliseconds(16));
            updateTimer.Elapsed += (_, _) => Update();
            updateTimer.DisposeWith(d);
            _profileEditorService.Playing.Subscribe(_ => _lastUpdate = DateTime.Now).DisposeWith(d);
            _profileEditorService.Playing.Subscribe(p => updateTimer.Enabled = p).DisposeWith(d);
            _lastUpdate = DateTime.MinValue;

            Disposable.Create(() =>
            {
                _settingsService.GetSetting("ProfileEditor.RepeatTimeline", true).Value = _repeating && _repeatTimeline;
                _settingsService.GetSetting("ProfileEditor.RepeatSegment", false).Value = _repeating && _repeatSegment;
            }).DisposeWith(d);
        });

        PlayFromStart = ReactiveCommand.Create(ExecutePlayFromStart, this.WhenAnyValue(vm => vm.KeyBindingsEnabled));
        TogglePlay = ReactiveCommand.Create(ExecuteTogglePlay, this.WhenAnyValue(vm => vm.KeyBindingsEnabled));
        GoToStart = ReactiveCommand.Create(ExecuteGoToStart);
        GoToEnd = ReactiveCommand.Create(ExecuteGoToEnd);
        GoToPreviousFrame = ReactiveCommand.Create(ExecuteGoToPreviousFrame);
        GoToNextFrame = ReactiveCommand.Create(ExecuteGoToNextFrame);
        CycleRepeating = ReactiveCommand.Create(ExecuteCycleRepeating);
    }

    public TimeSpan CurrentTime => _currentTime?.Value ?? TimeSpan.Zero;
    public string? FormattedCurrentTime => _formattedCurrentTime?.Value;
    public bool Playing => _playing?.Value ?? false;
    public bool KeyBindingsEnabled => _keyBindingsEnabled?.Value ?? false;

    public ReactiveCommand<Unit, Unit> PlayFromStart { get; }
    public ReactiveCommand<Unit, Unit> TogglePlay { get; }
    public ReactiveCommand<Unit, Unit> GoToStart { get; }
    public ReactiveCommand<Unit, Unit> GoToEnd { get; }
    public ReactiveCommand<Unit, Unit> GoToPreviousFrame { get; }
    public ReactiveCommand<Unit, Unit> GoToNextFrame { get; }
    public ReactiveCommand<Unit, Unit> CycleRepeating { get; }

    private void ExecutePlayFromStart()
    {
        ExecuteGoToStart();
        if (!Playing)
            _profileEditorService.Play();
    }

    private void ExecuteTogglePlay()
    {
        if (!Playing)
            _profileEditorService.Play();
        else
            _profileEditorService.Pause();
    }

    private void ExecuteGoToStart()
    {
        _profileEditorService.ChangeTime(TimeSpan.Zero);
    }

    private void ExecuteGoToEnd()
    {
        if (_profileElement == null)
            return;

        _profileEditorService.ChangeTime(_profileElement.Timeline.EndSegmentEndPosition);
    }

    private void ExecuteGoToPreviousFrame()
    {
        if (_profileElement == null)
            return;

        double frameTime = 1000.0 / _settingsService.GetSetting("Core.TargetFrameRate", 30).Value;
        double newTime = Math.Max(0, Math.Round((CurrentTime.TotalMilliseconds - frameTime) / frameTime) * frameTime);
        _profileEditorService.ChangeTime(TimeSpan.FromMilliseconds(newTime));
    }

    private void ExecuteGoToNextFrame()
    {
        if (_profileElement == null)
            return;

        double frameTime = 1000.0 / _settingsService.GetSetting("Core.TargetFrameRate", 30).Value;
        double newTime = Math.Round((CurrentTime.TotalMilliseconds + frameTime) / frameTime) * frameTime;
        newTime = Math.Min(newTime, _profileElement.Timeline.EndSegmentEndPosition.TotalMilliseconds);
        _profileEditorService.ChangeTime(TimeSpan.FromMilliseconds(newTime));
    }

    private void ExecuteCycleRepeating()
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
            this.RaisePropertyChanged(nameof(Repeating));
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

    private void Update()
    {
        try
        {
            if (_lastUpdate == DateTime.MinValue)
                _lastUpdate = DateTime.Now;

            TimeSpan newTime = CurrentTime.Add(DateTime.Now - _lastUpdate);
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

            Dispatcher.UIThread.Invoke(() => _profileEditorService.ChangeTime(newTime));
        }
        finally
        {
            _lastUpdate = DateTime.Now;
        }
    }
}