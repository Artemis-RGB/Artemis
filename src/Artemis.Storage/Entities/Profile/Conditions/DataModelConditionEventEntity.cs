using Artemis.Storage.Entities.Profile.Abstract;

namespace Artemis.Storage.Entities.Profile.Conditions
{
    public class DataModelConditionEventEntity : DataModelConditionPartEntity
    {
        public DataModelConditionEventEntity()
        {
           
        }

        public DataModelPathEntity EventPath { get; set; }
    }
}