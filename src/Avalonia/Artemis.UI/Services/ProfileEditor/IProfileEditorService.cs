using System;
using Artemis.Core;
using Artemis.UI.Services.Interfaces;

namespace Artemis.UI.Services.ProfileEditor
{
    public interface IProfileEditorService : IArtemisUIService
    {
        IObservable<ProfileConfiguration?> ProfileConfiguration { get; }
        IObservable<RenderProfileElement?> ProfileElement { get; }
        IObservable<ProfileEditorHistory?> History { get; }

        void ChangeCurrentProfileConfiguration(ProfileConfiguration? profileConfiguration);
        void ChangeCurrentProfileElement(RenderProfileElement? renderProfileElement);
        void ExecuteCommand(IProfileEditorCommand command);
    }
}