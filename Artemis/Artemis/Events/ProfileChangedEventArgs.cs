using System;
using Artemis.Profiles;

namespace Artemis.Events
{
    public class ProfileChangedEventArgs : EventArgs
    {
        public ProfileChangedEventArgs(ProfileModel profileModel)
        {
            ProfileModel = profileModel;
        }

        public ProfileModel ProfileModel { get; }
    }
}