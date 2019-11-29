using Artemis.Core.Models.Profile;

namespace Artemis.UI.Events
{
    public class ProfileEditorSelectedProfileChanged
    {
        public ProfileEditorSelectedProfileChanged(Profile profile)
        {
            Profile = profile;
        }

        public Profile Profile { get; }
    }
}