using System.Linq;
using Artemis.Layers.Interfaces;
using Artemis.Models.Interfaces;
using Artemis.Models.Profiles;

namespace Artemis.Layers.Conditions
{
    public class DataModelCondition : ILayerCondition
    {
        public bool ConditionsMet<T>(IDataModel dataModel, LayerModel layer)
        {
            return layer.Properties.Conditions.All(cm => cm.ConditionMet<T>(dataModel));
        }
    }
}