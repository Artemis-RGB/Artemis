using Artemis.Storage.Entities.Profile.Conditions;

namespace Artemis.Storage.Entities.Profile.DataBindings
{
    public class DataBindingConditionEntity
    {
        public string Value { get; set; }
        public DataModelConditionGroupEntity Condition { get; set; }
        public int Order { get; set; }
    }
}