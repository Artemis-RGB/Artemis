using System.Linq;
using Artemis.Models.Interfaces;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Models;

namespace Artemis.Profiles.Layers.Conditions
{
    public class EventCondition : ILayerCondition
    {
        public bool ConditionsMet(LayerModel layer, IDataModel dataModel)
        {
            lock (layer.Properties.Conditions)
            {
                var conditionsMet = layer.Properties.Conditions.All(cm => cm.ConditionMet(dataModel));
                layer.EventProperties.Update(layer, conditionsMet);

                if (conditionsMet && layer.EventProperties.CanTrigger)
                    layer.EventProperties.TriggerEvent(layer);

                return layer.EventProperties.MustDraw;
            }
        }
    }
}