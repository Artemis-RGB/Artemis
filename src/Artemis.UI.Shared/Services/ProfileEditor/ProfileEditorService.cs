using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared.Services.MainWindow;
using Artemis.UI.Shared.Services.ProfileEditor.Commands;
using DynamicData;
using Serilog;

namespace Artemis.UI.Shared.Services.ProfileEditor;

internal class ProfileEditorService : IProfileEditorService
{
    private readonly BehaviorSubject<ProfileEditorFocusMode> _focusModeSubject = new(ProfileEditorFocusMode.None);
    private readonly ILayerBrushService _layerBrushService;
    private readonly BehaviorSubject<ILayerProperty?> _layerPropertySubject = new(null);
    private readonly ILogger _logger;
    private readonly IDeviceService _deviceService;
    private readonly IModuleService _moduleService;
    private readonly BehaviorSubject<int> _pixelsPerSecondSubject = new(120);
    private readonly BehaviorSubject<bool> _playingSubject = new(false);
    private readonly BehaviorSubject<ProfileConfiguration?> _profileConfigurationSubject = new(null);
    private readonly Dictionary<ProfileConfiguration, ProfileEditorHistory> _profileEditorHistories = new();
    private readonly BehaviorSubject<RenderProfileElement?> _profileElementSubject = new(null);
    private readonly IProfileService _profileService;
    private readonly SourceList<ILayerPropertyKeyframe> _selectedKeyframes;
    private readonly BehaviorSubject<bool> _suspendedEditingSubject = new(false);
    private readonly BehaviorSubject<TimeSpan> _timeSubject = new(TimeSpan.Zero);
    private readonly IWindowService _windowService;
    private ProfileEditorCommandScope? _profileEditorHistoryScope;

    public ProfileEditorService(ILogger logger,
        IDeviceService deviceService,
        IProfileService profileService,
        IModuleService moduleService,
        ILayerBrushService layerBrushService,
        IMainWindowService mainWindowService,
        IWindowService windowService)
    {
        _logger = logger;
        _deviceService = deviceService;
        _profileService = profileService;
        _moduleService = moduleService;
        _layerBrushService = layerBrushService;
        _windowService = windowService;

        _selectedKeyframes = new SourceList<ILayerPropertyKeyframe>();
        _selectedKeyframes.Connect().Bind(out ReadOnlyObservableCollection<ILayerPropertyKeyframe> selectedKeyframes).Subscribe();

        ProfileConfiguration = _profileConfigurationSubject.AsObservable();
        ProfileElement = _profileElementSubject.AsObservable();
        LayerProperty = _layerPropertySubject.AsObservable();
        History = Observable.Defer(() => Observable.Return(GetHistory(_profileConfigurationSubject.Value))).Concat(ProfileConfiguration.Select(GetHistory));
        Time = _timeSubject.AsObservable();
        Playing = _playingSubject.AsObservable();
        SuspendedEditing = _suspendedEditingSubject.AsObservable();
        PixelsPerSecond = _pixelsPerSecondSubject.AsObservable();
        FocusMode = _focusModeSubject.AsObservable();
        SelectedKeyframes = selectedKeyframes;

        // Observe executing, undoing and redoing commands and run the auto-save after 1 second
        History.Select(h => h?.Execute ?? Observable.Never<Unit>())
            .Switch()
            .Merge(History.Select(h => h?.Undo ?? Observable.Never<IProfileEditorCommand?>())
                .Switch()
                .Select(_ => Unit.Default))
            .Merge(History.Select(h => h?.Redo ?? Observable.Never<IProfileEditorCommand?>())
                .Switch()
                .Select(_ => Unit.Default))
            .Throttle(TimeSpan.FromSeconds(1))
            .SelectMany(async _ => await AutoSaveProfileAsync())
            .Subscribe();

        // When the main window closes, stop editing
        mainWindowService.MainWindowClosed += (_, _) => ChangeCurrentProfileConfiguration(null);
    }

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

    private async Task<Unit> AutoSaveProfileAsync()
    {
        try
        {
            await SaveProfileAsync();
        }
        catch (Exception e)
        {
            _windowService.ShowExceptionDialog("Failed to auto-save profile", e);
            _logger.Error(e, "Failed to auto-save profile");
            throw;
        }

        return Unit.Default;
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
            renderElement.OverrideTimelineAndApply(time);

            foreach (ProfileElement child in renderElement.Children)
                TickProfileElement(child, time);
        }
    }

    private void ApplyFocusMode()
    {
        if (_suspendedEditingSubject.Value)
            _profileService.FocusProfileElement = null;

        _profileService.FocusProfileElement = _focusModeSubject.Value switch
        {
            ProfileEditorFocusMode.None => null,
            ProfileEditorFocusMode.Folder => _profileElementSubject.Value?.Parent,
            ProfileEditorFocusMode.Selection => _profileElementSubject.Value,
            _ => _profileService.FocusProfileElement
        };
    }

    public IObservable<ProfileConfiguration?> ProfileConfiguration { get; }
    public IObservable<RenderProfileElement?> ProfileElement { get; }
    public IObservable<ILayerProperty?> LayerProperty { get; }
    public IObservable<ProfileEditorHistory?> History { get; }
    public IObservable<bool> SuspendedEditing { get; }
    public IObservable<TimeSpan> Time { get; }
    public IObservable<bool> Playing { get; }
    public IObservable<int> PixelsPerSecond { get; }
    public IObservable<ProfileEditorFocusMode> FocusMode { get; }
    public ReadOnlyObservableCollection<ILayerPropertyKeyframe> SelectedKeyframes { get; }

    public async Task ChangeCurrentProfileConfiguration(ProfileConfiguration? profileConfiguration)
    {
        ProfileConfiguration? previous = _profileConfigurationSubject.Value;
        if (ReferenceEquals(previous, profileConfiguration))
            return;

        _logger.Verbose("ChangeCurrentProfileConfiguration {profile}", profileConfiguration);

        // Stop playing and save the current profile
        Pause();
        await SaveProfileAsync();

        // Deselect whatever profile element was active
        ChangeCurrentProfileElement(null);
        ChangeSuspendedEditing(false);

        // Close the command scope if one was open
        _profileEditorHistoryScope?.Dispose();

        // Activate the profile and it's mode off of the UI thread
        await Task.Run(() =>
        {
            // Activate the profile if one was provided
            if (profileConfiguration != null)
                _profileService.ActivateProfile(profileConfiguration);
            // If there is no profile configuration or module, deliberately set the override to null
            _moduleService.SetActivationOverride(profileConfiguration?.Module);
        });

        _profileService.FocusProfile = profileConfiguration;
        _profileConfigurationSubject.OnNext(profileConfiguration);

        ChangeTime(TimeSpan.Zero);
        previous?.Profile?.Reset();
    }

    public void ChangeCurrentProfileElement(RenderProfileElement? renderProfileElement)
    {
        _selectedKeyframes.Clear();
        _profileElementSubject.OnNext(renderProfileElement);

        ApplyFocusMode();
        ChangeCurrentLayerProperty(null);
    }

    /// <inheritdoc />
    public void ChangeCurrentLayerProperty(ILayerProperty? layerProperty)
    {
        _layerPropertySubject.OnNext(layerProperty);
    }

    public void ChangeTime(TimeSpan time)
    {
        Tick(time);
        _timeSubject.OnNext(time);
    }

    public void ChangeSuspendedEditing(bool suspend)
    {
        if (_suspendedEditingSubject.Value == suspend)
            return;

        if (suspend)
        {
            Pause();
            _profileService.UpdateFocusProfile = true;
            _profileConfigurationSubject.Value?.Profile?.Reset();
        }
        else
        {
            if (_profileConfigurationSubject.Value != null)
                _profileService.UpdateFocusProfile = false;
            Tick(_timeSubject.Value);
        }

        _suspendedEditingSubject.OnNext(suspend);
        ApplyFocusMode();
    }

    public void ChangeFocusMode(ProfileEditorFocusMode focusMode)
    {
        if (_focusModeSubject.Value == focusMode)
            return;

        _focusModeSubject.OnNext(focusMode);
        ApplyFocusMode();
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

    /// <inheritdoc />
    public Folder CreateAndAddFolder(ProfileElement target)
    {
        if (target is Layer targetLayer)
        {
            Folder folder = new(targetLayer.Parent, targetLayer.Parent.GetNewFolderName());
            ExecuteCommand(new AddProfileElement(folder, targetLayer.Parent, targetLayer.Order));
            return folder;
        }
        else
        {
            Folder folder = new(target, target.GetNewFolderName());
            ExecuteCommand(new AddProfileElement(folder, target, 0));
            return folder;
        }
    }

    /// <inheritdoc />
    public Layer CreateAndAddLayer(ProfileElement target)
    {
        if (target is Layer targetLayer)
        {
            Layer layer = new(targetLayer.Parent, targetLayer.GetNewLayerName());
            _layerBrushService.ApplyDefaultBrush(layer);

            layer.AddLeds(_deviceService.EnabledDevices.SelectMany(d => d.Leds));
            ExecuteCommand(new AddProfileElement(layer, targetLayer.Parent, targetLayer.Order));

            return layer;
        }
        else
        {
            Layer layer = new(target, target.GetNewLayerName());
            _layerBrushService.ApplyDefaultBrush(layer);

            layer.AddLeds(_deviceService.EnabledDevices.SelectMany(d => d.Leds));
            ExecuteCommand(new AddProfileElement(layer, target, 0));

            return layer;
        }
    }

    public void ChangePixelsPerSecond(int pixelsPerSecond)
    {
        _pixelsPerSecondSubject.OnNext(pixelsPerSecond);
    }

    /// <inheritdoc />
    public async Task SaveProfileAsync()
    {
        Profile? profile = _profileConfigurationSubject.Value?.Profile;
        if (profile != null)
            await Task.Run(() => _profileService.SaveProfile(profile, true));
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
            Tick(_timeSubject.Value);
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