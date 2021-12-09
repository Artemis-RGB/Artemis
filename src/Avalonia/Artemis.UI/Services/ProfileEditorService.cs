using System;
using System.Diagnostics;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.UI.Services.Interfaces;
using ReactiveUI;

namespace Artemis.UI.Services
{
    public class ProfileEditorService : IProfileEditorService
    {
        private readonly ReactiveCommand<ProfileConfiguration?, ProfileConfiguration?> _changeCurrentProfileConfiguration;
        private readonly ReactiveCommand<RenderProfileElement?, RenderProfileElement?> _changeCurrentProfileElement;
        private ProfileConfiguration? _currentProfileConfiguration;
        private RenderProfileElement? _currentProfileElement;

        public ProfileEditorService()
        {
            _changeCurrentProfileConfiguration = ReactiveCommand.CreateFromObservable<ProfileConfiguration?, ProfileConfiguration?>(Observable.Return);
            _changeCurrentProfileElement = ReactiveCommand.CreateFromObservable<RenderProfileElement?, RenderProfileElement?>(Observable.Return);

            CurrentProfileConfiguration = Observable.Defer(() => Observable.Return(_currentProfileConfiguration)).Concat(_changeCurrentProfileConfiguration);
            CurrentProfileElement = Observable.Defer(() => Observable.Return(_currentProfileElement)).Concat(_changeCurrentProfileElement);
        }


        public IObservable<ProfileConfiguration?> CurrentProfileConfiguration { get; }
        public IObservable<RenderProfileElement?> CurrentProfileElement { get; }

        public void ChangeCurrentProfileConfiguration(ProfileConfiguration? profileConfiguration)
        {
            _currentProfileConfiguration = profileConfiguration;
            _changeCurrentProfileConfiguration.Execute(profileConfiguration).Subscribe();
        }

        public void ChangeCurrentProfileElement(RenderProfileElement? renderProfileElement)
        {
            _currentProfileElement = renderProfileElement;
            _changeCurrentProfileElement.Execute(renderProfileElement).Subscribe();
        }
    }

    public interface IProfileEditorService : IArtemisUIService
    {
        IObservable<ProfileConfiguration?> CurrentProfileConfiguration { get; }
        IObservable<RenderProfileElement?> CurrentProfileElement { get; }

        void ChangeCurrentProfileConfiguration(ProfileConfiguration? profileConfiguration);
        void ChangeCurrentProfileElement(RenderProfileElement? renderProfileElement);
    }
}