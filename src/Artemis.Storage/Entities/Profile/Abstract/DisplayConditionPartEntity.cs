using System;
using System.Collections.Generic;

namespace Artemis.Storage.Entities.Profile.Abstract
{
    public abstract class DisplayConditionPartEntity
    {
        public Guid Id { get; set; }
        public Guid ParentId { get; set; }

        public List<DisplayConditionPartEntity> Children { get; set; }
    }
}