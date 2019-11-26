namespace Artemis.UI.Screens.Module.ProfileEditor.ProfileElements.ProfileElement
{
    public class LayerViewModel : ProfileElementViewModel
    {
        public LayerViewModel(ProfileElementViewModel parent, Core.Models.Profile.Abstract.ProfileElement layer, ProfileEditorViewModel profileEditorViewModel) 
            : base(parent, layer, profileEditorViewModel)
        {
        }

        public override bool SupportsChildren => false;
    }
}