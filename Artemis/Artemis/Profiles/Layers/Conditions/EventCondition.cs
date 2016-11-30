using System.Linq;
using Artemis.Models.Interfaces;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Models;

namespace Artemis.Profiles.Layers.Conditions
{
    public class EventCondition : ILayerCondition
    {
        public bool ConditionsMet(LayerModel layerModel, IDataModel dataModel)
        {
            lock (layerModel.Properties.Conditions)
            {
                var conditionsMet = layerModel.Properties.Conditions.All(cm => cm.ConditionMet(dataModel));
                layerModel.EventProperties.Update(layerModel, conditionsMet);

                if (conditionsMet && layerModel.EventProperties.CanTrigger)
                    layerModel.EventProperties.TriggerEvent(layerModel);

                return layerModel.EventProperties.MustDraw;
            }
        }
    }
}