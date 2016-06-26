using System.Linq;
using Artemis.Layers.Interfaces;
using Artemis.Models.Interfaces;
using Artemis.Models.Profiles;

namespace Artemis.Layers.Conditions
{
    public class EventCondition : ILayerCondition
    {
        public bool ConditionsMet<T>(IDataModel dataModel, LayerModel layer)
        {
            var conditionsMet = layer.Properties.Conditions.All(cm => cm.ConditionMet<T>(dataModel));
            layer.EventProperties.Update(layer, conditionsMet);

            if (conditionsMet && layer.EventProperties.MustTrigger)
                layer.EventProperties.TriggerEvent(layer);

            return conditionsMet && layer.EventProperties.MustDraw;
        }
    }
}