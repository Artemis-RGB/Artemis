using System.Linq;
using Artemis.Models.Interfaces;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Models;

namespace Artemis.Profiles.Layers.Conditions
{
    public class DataModelCondition : ILayerCondition
    {
        public bool ConditionsMet(LayerModel layerModel, IDataModel dataModel)
        {
            lock (layerModel.Properties.Conditions)
            {
                return layerModel.Properties.Conditions.All(cm => cm.ConditionMet(dataModel));
            }
        }
    }
}