using System.Linq;
using Artemis.Models.Interfaces;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Models;

namespace Artemis.Profiles.Layers.Conditions
{
    public class DataModelCondition : ILayerCondition
    {
        public bool ConditionsMet(LayerModel layer, IDataModel dataModel)
        {
            return layer.Properties.Conditions.All(cm => cm.ConditionMet(dataModel));
        }
    }
}