using Artemis.UI.Screens.Module.ProfileEditor.ProfileElements.ProfileElement;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor
{
    public class ProfileEditorPanelViewModel : Screen
    {
        public ProfileEditorViewModel ProfileEditorViewModel { get; set; }

        public virtual void ActiveProfileChanged()
        {
        }

        public virtual void ActiveProfileUpdated()
        {
        }

        public virtual void ProfileElementSelected(ProfileElementViewModel profileElement)
        {
        }
    }
}