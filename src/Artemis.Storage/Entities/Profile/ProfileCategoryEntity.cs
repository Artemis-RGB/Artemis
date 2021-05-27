using System;
using System.Collections.Generic;

namespace Artemis.Storage.Entities.Profile
{
    public class ProfileCategoryEntity
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
        public bool IsCollapsed { get; set; }
        public bool IsSuspended { get; set; }

        public List<ProfileConfigurationEntity> ProfileConfigurations { get; set; } = new();
    }
}