using System;
using Artemis.Core.Models.Profile;

namespace Artemis.UI.Events
{
    public class ProfileElementEventArgs : EventArgs
    {
        public ProfileElementEventArgs(ProfileElement profileElement)
        {
            ProfileElement = profileElement;
        }

        public ProfileElementEventArgs(ProfileElement profileElement, ProfileElement previousProfileElement)
        {
            ProfileElement = profileElement;
            PreviousProfileElement = previousProfileElement;
        }

        public ProfileElement ProfileElement { get; }
        public ProfileElement PreviousProfileElement { get; }
    }
}