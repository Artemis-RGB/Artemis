using System;
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
        public string ModuleId { get; set; }

        public Guid ProfileCategoryId { get; set; }
        public Guid ProfileId { get; set; }
    }
}