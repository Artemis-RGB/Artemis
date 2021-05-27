using System;
using System.Collections.Generic;
using Artemis.Storage.Entities.Profile.Conditions;

namespace Artemis.Storage.Entities.Profile
{
    public class ProfileConfigurationEntity
    {
        public Guid Id { get; set; }
        
        public string Name { get; set; }
        public string Icon { get; set; }
        public bool IsSuspended { get; set; }
        public int ActivationBehaviour { get; set; }

        public DataModelConditionGroupEntity ActivationCondition { get; set; }
        public List<string> Modules { get; set; } = new();

        public Guid ProfileCategoryId { get; set; }
        public Guid ProfileId { get; set; }
    }
}