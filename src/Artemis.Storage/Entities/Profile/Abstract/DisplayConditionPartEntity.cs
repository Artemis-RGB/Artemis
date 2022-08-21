using System.Collections.Generic;

namespace Artemis.Storage.Entities.Profile.Abstract;

public abstract class DataModelConditionPartEntity
{
    public List<DataModelConditionPartEntity> Children { get; set; }
}