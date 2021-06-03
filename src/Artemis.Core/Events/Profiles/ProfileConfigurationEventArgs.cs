using System;

namespace Artemis.Core
{
    /// <summary>
    ///     Provides data for profile configuration events.
    /// </summary>
    public class ProfileConfigurationEventArgs : EventArgs
    {
        internal ProfileConfigurationEventArgs(ProfileConfiguration profileConfiguration)
        {
            ProfileConfiguration = profileConfiguration;
        }

        /// <summary>
        ///     Gets the profile configuration this event is related to
        /// </summary>
        public ProfileConfiguration ProfileConfiguration { get; }
    }
}