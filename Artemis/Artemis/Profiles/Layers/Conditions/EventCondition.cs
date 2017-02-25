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

                layerModel.EventProperties.Update(layerModel, conditionsMet);

                if (conditionsMet)
                    layerModel.EventProperties.TriggerEvent(layerModel);

                return layerModel.EventProperties.MustDraw;
            }
        }
    }
}
