using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.Abstract;

namespace Artemis.UI.Events
{
    public class ProfileEditorSelectedElementChanged
    {
        public ProfileEditorSelectedElementChanged(ProfileElement profileElement)
        {
            ProfileElement = profileElement;
        }

        public ProfileElement ProfileElement { get; }
    }
}