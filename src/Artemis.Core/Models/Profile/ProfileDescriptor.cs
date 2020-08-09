using System;
using Artemis.Core.Plugins.Abstract;
using Artemis.Storage.Entities.Profile;

namespace Artemis.Core.Models.Profile
{
    public class ProfileDescriptor
    {
        internal ProfileDescriptor(ProfileModule profileModule, ProfileEntity profileEntity)
        {
            ProfileModule = profileModule;
            ProfileEntity = profileEntity;

            Id = profileEntity.Id;
            Name = profileEntity.Name;
        }
        
        public Guid Id { get; }
        public ProfileModule ProfileModule { get; }
        public string Name { get; }

        internal ProfileEntity ProfileEntity { get; }
    }
}