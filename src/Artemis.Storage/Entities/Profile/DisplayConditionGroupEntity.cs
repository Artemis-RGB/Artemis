using System;
using System.Collections.Generic;
using System.Text;
using Artemis.Storage.Entities.Profile.Abstract;

namespace Artemis.Storage.Entities.Profile
{
    public class DisplayConditionGroupEntity : DisplayConditionPartEntity
    {
        public DisplayConditionGroupEntity()
        {
            Children = new List<DisplayConditionPartEntity>();
        }

        public int BooleanOperator { get; set; }
    }
}