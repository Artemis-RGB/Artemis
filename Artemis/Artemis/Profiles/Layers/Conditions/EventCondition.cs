﻿using System.Linq;
using Artemis.Models.Interfaces;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Models;

namespace Artemis.Profiles.Layers.Conditions
{
    public class EventCondition : ILayerCondition
    {
        public bool ConditionsMet(LayerModel layer, IDataModel dataModel)
        {
            var conditionsMet = layer.Properties.Conditions.All(cm => cm.ConditionMet(dataModel));
            layer.EventProperties.Update(layer, conditionsMet);

            if (conditionsMet && layer.EventProperties.MustTrigger)
                layer.EventProperties.TriggerEvent(layer);

            return conditionsMet && layer.EventProperties.MustDraw;
        }
    }
}