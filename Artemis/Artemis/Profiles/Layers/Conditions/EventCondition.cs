using System;
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
                var conditionsMet = false;
                switch (layerModel.Properties.ConditionType)
                {
                    case ConditionType.AnyMet:
                        conditionsMet = layerModel.Properties.Conditions.Any(cm => cm.ConditionMet(dataModel));
                        break;
                    case ConditionType.AllMet:
                        conditionsMet = layerModel.Properties.Conditions.All(cm => cm.ConditionMet(dataModel));
                        break;
                    case ConditionType.NoneMet:
                        conditionsMet = !layerModel.Properties.Conditions.Any(cm => cm.ConditionMet(dataModel));
                        break;
                }
                layerModel.EventProperties.Update(layerModel, conditionsMet);

                if (conditionsMet && layerModel.EventProperties.CanTrigger)
                    layerModel.EventProperties.TriggerEvent(layerModel);

                return layerModel.EventProperties.MustDraw;
            }
        }
    }
}