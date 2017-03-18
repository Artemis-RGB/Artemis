using System.Linq;
using Artemis.Modules.Abstract;
using Artemis.Profiles.Layers.Abstract;
using Artemis.Profiles.Layers.Models;

namespace Artemis.Profiles.Layers.Conditions
{
    public class EventCondition : ILayerCondition
    {
        public bool ConditionsMet(LayerModel layerModel, ModuleDataModel dataModel)
        {
            lock (layerModel.Properties.Conditions)
            {
                var checkConditions = layerModel.Properties.Conditions.Where(c => c.Field != null).ToList();

                // Don't trigger constantly when there are no conditions
                if (!checkConditions.Any())
                {
                    layerModel.EventProperties.Update(layerModel, false);
                    return layerModel.EventProperties.MustDraw;
                }

                // Determine whether conditions are met
                var conditionsMet = false;
                switch (layerModel.Properties.ConditionType)
                {
                    case ConditionType.AnyMet:
                        conditionsMet = checkConditions.Any(cm => cm.ConditionMet(dataModel));
                        break;
                    case ConditionType.AllMet:
                        conditionsMet = checkConditions.All(cm => cm.ConditionMet(dataModel));
                        break;
                    case ConditionType.NoneMet:
                        conditionsMet = !checkConditions.Any(cm => cm.ConditionMet(dataModel));
                        break;
                }

                // Update the event properties
                layerModel.EventProperties.Update(layerModel, conditionsMet);
                
                // If conditions are met trigger the event, this won't do anything if the event isn't ready to be triggered yet
                if (conditionsMet)
                    layerModel.EventProperties.TriggerEvent(layerModel);

                // Return the event's MustDraw
                return layerModel.EventProperties.MustDraw;
            }
        }
    }
}
