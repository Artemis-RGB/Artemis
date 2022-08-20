using System;
using Artemis.Core;

namespace Artemis.UI.Shared.Events
{
    /// <summary>
    ///     Provides data on profile related events raised by the profile editor
    /// </summary>
    public class ProfileConfigurationEventArgs : EventArgs
    {
        internal ProfileConfigurationEventArgs(ProfileConfiguration? profileConfiguration)
        {
            ProfileConfiguration = profileConfiguration;
        }

        internal ProfileConfigurationEventArgs(ProfileConfiguration? profileConfiguration, ProfileConfiguration? previousProfileConfiguration)
        {
            ProfileConfiguration = profileConfiguration;
            PreviousProfileConfiguration = previousProfileConfiguration;
        }

        /// <summary>
        ///     Gets the profile the event was raised for
        /// </summary>
        public ProfileConfiguration? ProfileConfiguration { get; }

        /// <summary>
        ///     If applicable, the previous active profile before the event was raised
        /// </summary>
        public ProfileConfiguration? PreviousProfileConfiguration { get; }
    }
}