using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared.Services.Interfaces;
using Artemis.UI.Shared.Services.ProfileEditor.Commands;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using Serilog;

namespace Artemis.UI.Shared.Services.ProfileEditor;

internal class ProfileEditorService : IProfileEditorService
{
    private readonly ILogger _logger;
    private readonly IModuleService _moduleService;
    private readonly BehaviorSubject<int> _pixelsPerSecondSubject = new(120);
    private readonly BehaviorSubject<bool> _playingSubject = new(false);
    private readonly BehaviorSubject<ProfileConfiguration?> _profileConfigurationSubject = new(null);
    private readonly Dictionary<ProfileConfiguration, ProfileEditorHistory> _profileEditorHistories = new();
    private readonly BehaviorSubject<RenderProfileElement?> _profileElementSubject = new(null);
    private readonly IProfileService _profileService;
    private readonly SourceList<ILayerPropertyKeyframe> _selectedKeyframes = new();
    private readonly BehaviorSubject<bool> _suspendedEditingSubject = new(false);
    private readonly BehaviorSubject<TimeSpan> _timeSubject = new(TimeSpan.Zero);
    private readonly IWindowService _windowService;
    private ProfileEditorCommandScope? _profileEditorHistoryScope;

    public ProfileEditorService(ILogger logger, IProfileService profileService, IModuleService moduleService, IWindowService windowService)
    {
        _logger = logger;
        _profileService = profileService;
        _moduleService = moduleService;
        _windowService = windowService;

        ProfileConfiguration = _profileConfigurationSubject.AsObservable();
        ProfileElement = _profileElementSubject.AsObservable();
        History = Observable.Defer(() => Observable.Return(GetHistory(_profileConfigurationSubject.Value))).Concat(ProfileConfiguration.Select(GetHistory));
        Time = _timeSubject.AsObservable();
        Playing = _playingSubject.AsObservable();
        SuspendedEditing = _suspendedEditingSubject.AsObservable();
        PixelsPerSecond = _pixelsPerSecondSubject.AsObservable();
        Tools = new SourceList<IToolViewModel>();
        Tools.Connect().AutoRefreshOnObservable(t => t.WhenAnyValue(vm => vm.IsSelected)).Subscribe(set =>
        {
            IToolViewModel? changed = set.FirstOrDefault()?.Item.Current;
            if (changed == null)
                return;

            // Disable all others if the changed one is selected and exclusive
            if (changed.IsSelected && changed.IsExclusive)
            {
                Tools.Edit(list =>
                {
                    foreach (IToolViewModel toolViewModel in list.Where(t => t.IsExclusive && t != changed))
                        toolViewModel.IsSelected = false;
                });
            }
        });
    }

    public IObservable<bool> SuspendedEditing { get; }
    public IObservable<ProfileConfiguration?> ProfileConfiguration { get; }
    public IObservable<RenderProfileElement?> ProfileElement { get; }
    public IObservable<ProfileEditorHistory?> History { get; }
    public IObservable<TimeSpan> Time { get; }
    public IObservable<bool> Playing { get; }
    public IObservable<int> PixelsPerSecond { get; }
    public SourceList<IToolViewModel> Tools { get; }

    private ProfileEditorHistory? GetHistory(ProfileConfiguration? profileConfiguration)
    {
        if (profileConfiguration == null)
            return null;
        if (_profileEditorHistories.TryGetValue(profileConfiguration, out ProfileEditorHistory? history))
            return history;

        ProfileEditorHistory newHistory = new(profileConfiguration);
        _profileEditorHistories.Add(profileConfiguration, newHistory);
        return newHistory;
    }

    private void Tick(TimeSpan time)
    {
        if (_profileConfigurationSubject.Value?.Profile == null || _suspendedEditingSubject.Value)
            return;

        TickProfileElement(_profileConfigurationSubject.Value.Profile.GetRootFolder(), time);
    }

    private void TickProfileElement(ProfileElement profileElement, TimeSpan time)
    {
        if (profileElement is not RenderProfileElement renderElement)
            return;

        if (renderElement.Suspended)
        {
            renderElement.Disable();
        }
        else
        {
            renderElement.Enable();
            bool stickToMainSegment = (renderElement != _profileElementSubject.Value || renderElement.Timeline.Length < time) && renderElement.Timeline.PlayMode == TimelinePlayMode.Repeat;
            renderElement.OverrideTimelineAndApply(time, stickToMainSegment);

            foreach (ProfileElement child in renderElement.Children)
                TickProfileElement(child, time);
        }
    }

    public IObservable<IChangeSet<ILayerPropertyKeyframe>> ConnectToKeyframes()
    {
        return _selectedKeyframes.Connect();
    }

    public void ChangeCurrentProfileConfiguration(ProfileConfiguration? profileConfiguration)
    {
        if (ReferenceEquals(_profileConfigurationSubject.Value, profileConfiguration))
            return;

        _logger.Verbose("ChangeCurrentProfileConfiguration {profile}", profileConfiguration);

        // Stop playing and save the current profile
        Pause();
        if (_profileConfigurationSubject.Value?.Profile != null)
            _profileConfigurationSubject.Value.Profile.LastSelectedProfileElement = _profileElementSubject.Value;
        SaveProfile();

        // No need to deactivate the profile, if needed it will be deactivated next update
        if (_profileConfigurationSubject.Value != null)
            _profileConfigurationSubject.Value.IsBeingEdited = false;

        // Deselect whatever profile element was active
        ChangeCurrentProfileElement(null);

        // Close the command scope if one was open
        _profileEditorHistoryScope?.Dispose();

        // The new profile may need activation
        if (profileConfiguration != null)
        {
            profileConfiguration.IsBeingEdited = true;
            _moduleService.SetActivationOverride(profileConfiguration.Module);
            _profileService.ActivateProfile(profileConfiguration);
            _profileService.RenderForEditor = true;

            if (profileConfiguration.Profile?.LastSelectedProfileElement is RenderProfileElement renderProfileElement)
                ChangeCurrentProfileElement(renderProfileElement);
        }
        else
        {
            _moduleService.SetActivationOverride(null);
            _profileService.RenderForEditor = false;
        }

        _profileConfigurationSubject.OnNext(profileConfiguration);
        ChangeTime(TimeSpan.Zero);
    }

    public void ChangeCurrentProfileElement(RenderProfileElement? renderProfileElement)
    {
        _selectedKeyframes.Clear();
        _profileElementSubject.OnNext(renderProfileElement);
    }

    public void ChangeTime(TimeSpan time)
    {
        Tick(time);
        _timeSubject.OnNext(time);
    }

    public void SelectKeyframe(ILayerPropertyKeyframe? keyframe, bool expand, bool toggle)
    {
        if (keyframe == null)
        {
            if (!expand)
                _selectedKeyframes.Clear();
            return;
        }

        if (toggle)
        {
            // Toggle only the clicked keyframe, leave others alone
            if (_selectedKeyframes.Items.Contains(keyframe))
                _selectedKeyframes.Remove(keyframe);
            else
                _selectedKeyframes.Add(keyframe);
        }
        else
        {
            if (expand)
                _selectedKeyframes.Add(keyframe);
            else
                _selectedKeyframes.Edit(l =>
                {
                    l.Clear();
                    l.Add(keyframe);
                });
        }
    }

    public void SelectKeyframes(IEnumerable<ILayerPropertyKeyframe> keyframes, bool expand)
    {
        if (expand)
        {
            List<ILayerPropertyKeyframe> toAdd = keyframes.Except(_selectedKeyframes.Items).ToList();
            _selectedKeyframes.AddRange(toAdd);
        }
        else
        {
            _selectedKeyframes.Edit(l =>
            {
                l.Clear();
                l.AddRange(keyframes);
            });
        }
    }

    public TimeSpan SnapToTimeline(TimeSpan time, TimeSpan tolerance, bool snapToSegments, bool snapToCurrentTime, List<TimeSpan>? snapTimes = null)
    {
        RenderProfileElement? profileElement = _profileElementSubject.Value;
        if (snapToSegments && profileElement != null)
        {
            // Snap to the end of the start segment
            if (Math.Abs(time.TotalMilliseconds - profileElement.Timeline.StartSegmentEndPosition.TotalMilliseconds) < tolerance.TotalMilliseconds)
                return profileElement.Timeline.StartSegmentEndPosition;

            // Snap to the end of the main segment
            if (Math.Abs(time.TotalMilliseconds - profileElement.Timeline.MainSegmentEndPosition.TotalMilliseconds) < tolerance.TotalMilliseconds)
                return profileElement.Timeline.MainSegmentEndPosition;

            // Snap to the end of the end segment (end of the timeline)
            if (Math.Abs(time.TotalMilliseconds - profileElement.Timeline.EndSegmentEndPosition.TotalMilliseconds) < tolerance.TotalMilliseconds)
                return profileElement.Timeline.EndSegmentEndPosition;
        }

        // Snap to the current time
        if (snapToCurrentTime)
            if (Math.Abs(time.TotalMilliseconds - _timeSubject.Value.TotalMilliseconds) < tolerance.TotalMilliseconds)
                return _timeSubject.Value;

        if (snapTimes != null)
        {
            // Find the closest keyframe
            TimeSpan closeSnapTime = snapTimes.FirstOrDefault(s => Math.Abs(time.TotalMilliseconds - s.TotalMilliseconds) < tolerance.TotalMilliseconds)!;
            if (closeSnapTime != TimeSpan.Zero)
                return closeSnapTime;
        }

        return time;
    }

    public TimeSpan RoundTime(TimeSpan time)
    {
        // Round the time to something that fits the current zoom level
        if (_pixelsPerSecondSubject.Value < 50)
            return TimeSpan.FromMilliseconds(Math.Round(time.TotalMilliseconds / 200.0) * 200.0);
        if (_pixelsPerSecondSubject.Value < 100)
            return TimeSpan.FromMilliseconds(Math.Round(time.TotalMilliseconds / 100.0) * 100.0);
        if (_pixelsPerSecondSubject.Value < 200)
            return TimeSpan.FromMilliseconds(Math.Round(time.TotalMilliseconds / 50.0) * 50.0);
        if (_pixelsPerSecondSubject.Value < 500)
            return TimeSpan.FromMilliseconds(Math.Round(time.TotalMilliseconds / 20.0) * 20.0);
        return TimeSpan.FromMilliseconds(Math.Round(time.TotalMilliseconds));
    }

    public void ChangePixelsPerSecond(int pixelsPerSecond)
    {
        _pixelsPerSecondSubject.OnNext(pixelsPerSecond);
    }


    /// <inheritdoc />
    public void SaveProfile()
    {
        Profile? profile = _profileConfigurationSubject.Value?.Profile;
        if (profile == null)
            return;

        _profileService.SaveProfile(profile, true);
    }

    /// <inheritdoc />
    public async Task SaveProfileAsync()
    {
        await Task.Run(SaveProfile);
    }

    /// <inheritdoc />
    public void Play()
    {
        if (!_playingSubject.Value)
            _playingSubject.OnNext(true);
    }

    /// <inheritdoc />
    public void Pause()
    {
        if (_playingSubject.Value)
            _playingSubject.OnNext(false);
    }

    #region Commands

    public void ExecuteCommand(IProfileEditorCommand command)
    {
        try
        {
            ProfileEditorHistory? history = GetHistory(_profileConfigurationSubject.Value);
            if (history == null)
                throw new ArtemisSharedUIException("Can't execute a command when there's no active profile configuration");

            // If a scope is active add the command to it, the scope will execute it immediately
            if (_profileEditorHistoryScope != null)
            {
                _profileEditorHistoryScope.AddCommand(command);
                return;
            }

            history.Execute.Execute(command).Subscribe();
        }
        catch (Exception e)
        {
            _windowService.ShowExceptionDialog("Editor command failed", e);
            throw;
        }
    }

    public ProfileEditorCommandScope CreateCommandScope(string name)
    {
        if (_profileEditorHistoryScope != null)
            throw new ArtemisSharedUIException($"A command scope is already active, name: {_profileEditorHistoryScope.Name}.");

        if (name == null)
            throw new ArgumentNullException(nameof(name));

        _profileEditorHistoryScope = new ProfileEditorCommandScope(this, name);
        return _profileEditorHistoryScope;
    }

    internal void StopCommandScope()
    {
        // This might happen if the scope is disposed twice, it's no biggie
        if (_profileEditorHistoryScope == null)
            return;

        ProfileEditorCommandScope scope = _profileEditorHistoryScope;
        _profileEditorHistoryScope = null;

        // Executing the composite command won't do anything the first time (see last ctor variable)
        // commands were already executed each time they were added to the scope
        ExecuteCommand(new CompositeCommand(scope.ProfileEditorCommands, scope.Name, true));
    }

    #endregion
}