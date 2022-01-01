using System;

namespace Artemis.Core
{
    /// <summary>
    ///     Provides data for profile element events.
    /// </summary>
    public class ProfileElementEventArgs : EventArgs
    {
        internal ProfileElementEventArgs(ProfileElement profileElement)
        {
            ProfileElement = profileElement;
        }

        /// <summary>
        ///     Gets the profile element this event is related to
        /// </summary>
        public ProfileElement ProfileElement { get; }
    }
}