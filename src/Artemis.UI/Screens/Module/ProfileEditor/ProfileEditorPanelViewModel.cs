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
    }
}