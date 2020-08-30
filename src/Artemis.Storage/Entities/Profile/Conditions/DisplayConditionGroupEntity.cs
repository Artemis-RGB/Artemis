using System.Collections.Generic;
using Artemis.Storage.Entities.Profile.Abstract;

namespace Artemis.Storage.Entities.Profile.Conditions
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