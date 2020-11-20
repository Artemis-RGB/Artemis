using System;
using Artemis.Core;

namespace Artemis.UI.Shared
{
    /// <summary>
    ///     Provides data on profile related events raised by the profile editor
    /// </summary>
    public class ProfileEventArgs : EventArgs
    {
        internal ProfileEventArgs(Profile profile)
        {
            Profile = profile;
        }

        internal ProfileEventArgs(Profile profile, Profile previousProfile)
        {
            Profile = profile;
            PreviousProfile = previousProfile;
        }

        /// <summary>
        ///     Gets the profile the event was raised for
        /// </summary>
        public Profile Profile { get; }

        /// <summary>
        ///     If applicable, the previous active profile before the event was raised
        /// </summary>
        public Profile? PreviousProfile { get; }
    }
}