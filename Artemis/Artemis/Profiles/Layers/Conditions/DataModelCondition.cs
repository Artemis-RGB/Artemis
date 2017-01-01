using System;
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
                switch (layerModel.Properties.ConditionType)
                {
                    case ConditionType.AnyMet:
                        return layerModel.Properties.Conditions.Any(cm => cm.ConditionMet(dataModel));
                    case ConditionType.AllMet:
                        return layerModel.Properties.Conditions.All(cm => cm.ConditionMet(dataModel));
                    case ConditionType.NoneMet:
                        return !layerModel.Properties.Conditions.Any(cm => cm.ConditionMet(dataModel));
                    default:
                        return false;
                }
            }
        }
    }
}