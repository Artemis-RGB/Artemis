using System.Collections.Generic;
using Artemis.Storage.Entities.Profile.Abstract;

namespace Artemis.Storage.Entities.Profile.Conditions
{
    public class DataModelConditionGroupEntity : DataModelConditionPartEntity
    {
        public DataModelConditionGroupEntity()
        {
            Children = new List<DataModelConditionPartEntity>();
        }

        public int BooleanOperator { get; set; }
    }
}