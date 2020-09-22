using System.Collections.Generic;

namespace Artemis.Storage.Entities.Profile.DataBindings
{
    public class ConditionalDataBindingEntity : IDataBindingModeEntity
    {
        public ConditionalDataBindingEntity()
        {
            Values = new List<DataBindingConditionEntity>();
        }

        public List<DataBindingConditionEntity> Values { get; set; }
    }
}