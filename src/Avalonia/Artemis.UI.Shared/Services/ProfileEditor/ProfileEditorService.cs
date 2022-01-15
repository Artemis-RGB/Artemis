using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared.Services.Interfaces;
using Serilog;

namespace Artemis.UI.Shared.Services.ProfileEditor;

internal class ProfileEditorService : IProfileEditorService
{
    private readonly BehaviorSubject<ProfileConfiguration?> _profileConfigurationSubject = new(null);
    private readonly Dictionary<ProfileConfiguration, ProfileEditorHistory> _profileEditorHistories = new();
    private readonly BehaviorSubject<RenderProfileElement?> _profileElementSubject = new(null);
    private readonly BehaviorSubject<TimeSpan> _timeSubject = new(TimeSpan.Zero);
    private readonly BehaviorSubject<bool> _playingSubject = new(false);
    private readonly BehaviorSubject<bool> _suspendedEditingSubject = new(false);
    private readonly BehaviorSubject<double> _pixelsPerSecondSubject = new(300);
    private readonly ILogger _logger;
    private readonly IProfileService _profileService;
    private readonly IModuleService _moduleService;
    private readonly IWindowService _windowService;

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

    public IObservable<ProfileConfiguration?> ProfileConfiguration { get; }
    public IObservable<RenderProfileElement?> ProfileElement { get; }
    public IObservable<ProfileEditorHistory?> History { get; }
    public IObservable<TimeSpan> Time { get; }
    public IObservable<bool> Playing { get; }
    public IObservable<bool> SuspendedEditing { get; }
    public IObservable<double> PixelsPerSecond { get; }

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
    }

    public void ChangeCurrentProfileElement(RenderProfileElement? renderProfileElement)
    {
        _profileElementSubject.OnNext(renderProfileElement);
    }

    public void ChangeTime(TimeSpan time)
    {
        Tick(time);
        _timeSubject.OnNext(time);
    }

    public void ChangePixelsPerSecond(double pixelsPerSecond)
    {
        _pixelsPerSecondSubject.OnNext(pixelsPerSecond);
    }

    public void ExecuteCommand(IProfileEditorCommand command)
    {
        try
        {
            ProfileEditorHistory? history = GetHistory(_profileConfigurationSubject.Value);
            if (history == null)
                throw new ArtemisSharedUIException("Can't execute a command when there's no active profile configuration");

            history.Execute.Execute(command).Subscribe();
        }
        catch (Exception e)
        {
            _windowService.ShowExceptionDialog("Editor command failed", e);
            throw;
        }
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
            renderElement.Timeline.Override(
                time,
                (renderElement != _profileElementSubject.Value || renderElement.Timeline.Length < time) && renderElement.Timeline.PlayMode == TimelinePlayMode.Repeat
            );

            foreach (ProfileElement child in renderElement.Children)
                TickProfileElement(child, time);
        }
    }
}