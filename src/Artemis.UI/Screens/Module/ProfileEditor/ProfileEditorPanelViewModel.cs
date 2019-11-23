using Artemis.Core.Models.Profile;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor
{
    public class ProfileEditorPanelViewModel : Screen
    {
        public Profile Profile { get; set; }

        public virtual void OnProfileChanged()
        {
        }
    }
}