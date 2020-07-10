using System.Collections.Generic;

namespace Artemis.Storage.Entities.Profile.Abstract
{
    public abstract class DisplayConditionPartEntity
    {
        public List<DisplayConditionPartEntity> Children { get; set; }
    }
}