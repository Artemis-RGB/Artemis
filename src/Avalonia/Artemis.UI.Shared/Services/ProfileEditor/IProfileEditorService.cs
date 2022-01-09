using System;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Shared.Services.Interfaces;

namespace Artemis.UI.Shared.Services.ProfileEditor
{
    public interface IProfileEditorService : IArtemisSharedUIService
    {
        IObservable<ProfileConfiguration?> ProfileConfiguration { get; }
        IObservable<RenderProfileElement?> ProfileElement { get; }
        IObservable<ProfileEditorHistory?> History { get; }
        IObservable<TimeSpan> Time { get; }

        void ChangeCurrentProfileConfiguration(ProfileConfiguration? profileConfiguration);
        void ChangeCurrentProfileElement(RenderProfileElement? renderProfileElement);
        void ChangeTime(TimeSpan time);

        void ExecuteCommand(IProfileEditorCommand command);
        void SaveProfile();
        Task SaveProfileAsync();
    }
}