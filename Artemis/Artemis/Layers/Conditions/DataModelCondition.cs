using System.Linq;
using Artemis.Layers.Interfaces;
using Artemis.Models.Interfaces;
using Artemis.Models.Profiles;

namespace Artemis.Layers.Conditions
{
    public class DataModelCondition : ILayerCondition
    {
        public bool ConditionsMet(LayerModel layer, IDataModel dataModel)
        {
            return layer.Properties.Conditions.All(cm => cm.ConditionMet(dataModel));
        }
    }
}