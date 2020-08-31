using System;
using Artemis.Core;

namespace Artemis.UI.Shared
{
    public class ProfileEventArgs : EventArgs
    {
        public ProfileEventArgs(Profile profile)
        {
            Profile = profile;
        }

        public ProfileEventArgs(Profile profile, Profile previousProfile)
        {
            Profile = profile;
            PreviousProfile = previousProfile;
        }

        public Profile Profile { get; }
        public Profile PreviousProfile { get; }
    }
}