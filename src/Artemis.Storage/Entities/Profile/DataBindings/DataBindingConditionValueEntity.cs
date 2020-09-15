using Artemis.Storage.Entities.Profile.Conditions;

namespace Artemis.Storage.Entities.Profile.DataBindings
{
    public class DataBindingConditionValueEntity
    {
        public string Value { get; set; }
        public DisplayConditionGroupEntity RootGroup { get; set; }
    }
}