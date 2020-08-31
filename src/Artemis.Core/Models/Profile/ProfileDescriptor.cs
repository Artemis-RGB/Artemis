using System;
using Artemis.Core.Modules;
using Artemis.Storage.Entities.Profile;

namespace Artemis.Core
{
    public class ProfileDescriptor
    {
        internal ProfileDescriptor(ProfileModule profileModule, ProfileEntity profileEntity)
        {
            ProfileModule = profileModule;

            Id = profileEntity.Id;
            Name = profileEntity.Name;
            IsLastActiveProfile = profileEntity.IsActive;
        }

        public bool IsLastActiveProfile { get; set; }

        public Guid Id { get; }
        public ProfileModule ProfileModule { get; }
        public string Name { get; }
    }
}