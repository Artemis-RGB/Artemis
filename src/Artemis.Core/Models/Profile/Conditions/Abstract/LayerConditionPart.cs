using System.Collections.Generic;

namespace Artemis.Core.Models.Profile.Conditions.Abstract
{
    public abstract class LayerConditionPart
    {
        protected LayerConditionPart()
        {
            Children = new List<LayerConditionPart>();
        }

        public List<LayerConditionPart> Children { get; set; }
    }
}