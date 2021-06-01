using System;
using Artemis.Storage.Entities.Profile.Conditions;

namespace Artemis.Storage.Entities.Profile
{
    public class ProfileConfigurationEntity
    {
        public string Name { get; set; }
        public string MaterialIcon { get; set; }
        public Guid FileIconId { get; set; }
        public int IconType { get; set; }

        public bool IsSuspended { get; set; }
        public int ActivationBehaviour { get; set; }
        public DataModelConditionGroupEntity ActivationCondition { get; set; }

        public string ModuleId { get; set; }

        public Guid ProfileCategoryId { get; set; }
        public Guid ProfileId { get; set; }
    }
}