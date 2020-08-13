using System;
using System.Collections.Generic;
using Artemis.Storage.Entities.Profile.Abstract;

namespace Artemis.Storage.Entities.Profile
{
    public class DisplayConditionListEntity : DisplayConditionPartEntity
    {
        public DisplayConditionListEntity()
        {
            Children = new List<DisplayConditionPartEntity>();
        }

        public Guid? ListDataModelGuid { get; set; }
        public string ListPropertyPath { get; set; }

        public int ListOperator { get; set; }
    }
}