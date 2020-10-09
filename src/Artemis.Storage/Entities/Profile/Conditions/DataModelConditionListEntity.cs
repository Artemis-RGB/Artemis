using System;
using System.Collections.Generic;
using Artemis.Storage.Entities.Profile.Abstract;

namespace Artemis.Storage.Entities.Profile.Conditions
{
    public class DataModelConditionListEntity : DataModelConditionPartEntity
    {
        public DataModelConditionListEntity()
        {
            Children = new List<DataModelConditionPartEntity>();
        }

        public DataModelPathEntity ListPath { get; set; }
        public int ListOperator { get; set; }
    }
}