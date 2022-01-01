using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Artemis.Core;
using Artemis.UI.Exceptions;

namespace Artemis.UI.Services.ProfileEditor
{
    public class ProfileEditorService : IProfileEditorService
    {
        private readonly Dictionary<ProfileConfiguration, ProfileEditorHistory> _profileEditorHistories = new();
        private readonly BehaviorSubject<ProfileConfiguration?> _profileConfigurationSubject = new(null);
        private readonly BehaviorSubject<RenderProfileElement?> _profileElementSubject = new(null);

        public ProfileEditorService()
        {
            ProfileConfiguration = _profileConfigurationSubject.AsObservable().DistinctUntilChanged();
            ProfileElement = _profileElementSubject.AsObservable().DistinctUntilChanged();
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

        public void ChangeCurrentProfileConfiguration(ProfileConfiguration? profileConfiguration)
        {
            _profileConfigurationSubject.OnNext(profileConfiguration);
        }

        public void ChangeCurrentProfileElement(RenderProfileElement? renderProfileElement)
        {
            _profileElementSubject.OnNext(renderProfileElement);
        }

        public void ExecuteCommand(IProfileEditorCommand command)
        {
            ProfileEditorHistory? history = GetHistory(_profileConfigurationSubject.Value);
            if (history == null)
                throw new ArtemisUIException("Can't execute a command when there's no active profile configuration");

            history.Execute.Execute(command).Subscribe();
        }
    }
}