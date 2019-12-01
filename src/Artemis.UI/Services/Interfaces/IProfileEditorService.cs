using System;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.Abstract;
using Artemis.Core.Plugins.LayerElement;

namespace Artemis.UI.Services.Interfaces
{
    public interface IProfileEditorService : IArtemisUIService
    {
        Profile SelectedProfile { get; }
        ProfileElement SelectedProfileElement { get; }
        LayerElement SelectedLayerElement { get; }

        void ChangeSelectedProfile(Profile profile);
        void UpdateSelectedProfile();
        void ChangeSelectedProfileElement(ProfileElement profileElement);
        void UpdateSelectedProfileElement();
        void ChangeSelectedLayerElement(LayerElement layerElement);

        event EventHandler SelectedProfileChanged;
        event EventHandler SelectedProfileUpdated;
        event EventHandler SelectedProfileElementChanged;
        event EventHandler SelectedProfileElementUpdated;
        event EventHandler SelectedLayerElementChanged;
    }
}