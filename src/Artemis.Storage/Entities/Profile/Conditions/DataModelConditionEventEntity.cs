using System.Collections.Generic;
using Artemis.Storage.Entities.Profile.Abstract;

namespace Artemis.Storage.Entities.Profile.Conditions
{
    public class DataModelConditionEventEntity : DataModelConditionPartEntity
    {
        public DataModelConditionEventEntity()
        {
            Children = new List<DataModelConditionPartEntity>();
        }

        public DataModelPathEntity EventPath { get; set; }
    }
}