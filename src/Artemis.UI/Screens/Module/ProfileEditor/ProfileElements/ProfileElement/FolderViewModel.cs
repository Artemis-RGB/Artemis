namespace Artemis.UI.Screens.Module.ProfileEditor.ProfileElements.ProfileElement
{
    public class FolderViewModel : ProfileElementViewModel
    {
        public FolderViewModel(ProfileElementViewModel parent, Core.Models.Profile.Abstract.ProfileElement folder, ProfileEditorViewModel profileEditorViewModel)
            : base(parent, folder, profileEditorViewModel)
        {
        }

        public override bool SupportsChildren => true;
    }
}