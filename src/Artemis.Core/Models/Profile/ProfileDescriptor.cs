using System;
using Artemis.Core.Modules;
using Artemis.Storage.Entities.Profile;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a descriptor that describes a profile
    /// </summary>
    public class ProfileDescriptor : CorePropertyChanged
    {
        internal ProfileDescriptor(ProfileModule profileModule, ProfileEntity profileEntity)
        {
            ProfileModule = profileModule;

            Id = profileEntity.Id;
            Name = profileEntity.Name;
            IsLastActiveProfile = profileEntity.IsActive;
        }

        /// <summary>
        ///     Gets the module backing the profile
        /// </summary>
        public ProfileModule ProfileModule { get; }

        /// <summary>
        ///     Gets the unique ID of the profile by which it can be loaded from storage
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        ///     Gets the name of the profile
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets a boolean indicating whether this was the last active profile
        /// </summary>
        public bool IsLastActiveProfile { get; }
    }
}