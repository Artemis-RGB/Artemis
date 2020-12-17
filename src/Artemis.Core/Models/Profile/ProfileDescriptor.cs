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
        private readonly ProfileEntity _profileEntity;

        internal ProfileDescriptor(ProfileModule profileModule, ProfileEntity profileEntity)
        {
            _profileEntity = profileEntity;
            ProfileModule = profileModule;
        }

        /// <summary>
        ///     Gets the module backing the profile
        /// </summary>
        public ProfileModule ProfileModule { get; }

        /// <summary>
        ///     Gets the unique ID of the profile by which it can be loaded from storage
        /// </summary>
        public Guid Id => _profileEntity.Id;

        /// <summary>
        ///     Gets the name of the profile
        /// </summary>
        public string Name => _profileEntity.Name;

        /// <summary>
        ///     Gets a boolean indicating whether this was the last active profile
        /// </summary>
        public bool IsLastActiveProfile => _profileEntity.IsActive;

        /// <summary>
        ///     Triggers a property change for all properties linked to the backing profile entity ¯\_(ツ)_/¯
        /// </summary>
        public void Update()
        {
            OnPropertyChanged(nameof(Id));
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(IsLastActiveProfile));
        }
    }
}