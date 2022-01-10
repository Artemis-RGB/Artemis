using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared.Services.Interfaces;

namespace Artemis.UI.Shared.Services.ProfileEditor;

internal class ProfileEditorService : IProfileEditorService
{
    private readonly BehaviorSubject<ProfileConfiguration?> _profileConfigurationSubject = new(null);
    private readonly Dictionary<ProfileConfiguration, ProfileEditorHistory> _profileEditorHistories = new();
    private readonly BehaviorSubject<RenderProfileElement?> _profileElementSubject = new(null);
    private readonly BehaviorSubject<TimeSpan> _timeSubject = new(TimeSpan.Zero);
    private readonly IProfileService _profileService;
    private readonly IWindowService _windowService;

    public ProfileEditorService(IProfileService profileService, IWindowService windowService)
    {
        _profileService = profileService;
        _windowService = windowService;
        ProfileConfiguration = _profileConfigurationSubject.AsObservable();
        ProfileElement = _profileElementSubject.AsObservable();
        History = Observable.Defer(() => Observable.Return(GetHistory(_profileConfigurationSubject.Value))).Concat(ProfileConfiguration.Select(GetHistory));
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

    public void ChangeCurrentProfileConfiguration(ProfileConfiguration? profileConfiguration)
    {
        _profileConfigurationSubject.OnNext(profileConfiguration);
    }

    public void ChangeCurrentProfileElement(RenderProfileElement? renderProfileElement)
    {
        _profileElementSubject.OnNext(renderProfileElement);
    }

    public void ChangeTime(TimeSpan time)
    {
        _timeSubject.OnNext(time);
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
}