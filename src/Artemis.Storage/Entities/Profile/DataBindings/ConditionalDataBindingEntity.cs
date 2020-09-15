using System.Collections.Generic;

namespace Artemis.Storage.Entities.Profile.DataBindings
{
    public class ConditionalDataBindingEntity : IDataBindingModeEntity
    {
        public ConditionalDataBindingEntity()
        {
            Values = new List<DataBindingConditionValueEntity>();
        }

        public List<DataBindingConditionValueEntity> Values { get; set; }
    }
}