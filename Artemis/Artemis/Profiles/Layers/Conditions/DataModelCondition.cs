using System.Collections.Generic;
using System.Linq;
using Artemis.Modules.Abstract;
using Artemis.Profiles.Layers.Abstract;
using Artemis.Profiles.Layers.Models;

namespace Artemis.Profiles.Layers.Conditions
{
    public class DataModelCondition : LayerCondition
    {
        public override bool ConditionsMet(LayerModel layerModel, ModuleDataModel dataModel)
        {
            lock (layerModel.Properties.Conditions)
            {
                var checkConditions = layerModel.Properties.Conditions.Where(c => c.Field != null && !c.Field.Contains("hotkey")).ToList();

                if (checkConditions.Count == layerModel.Properties.Conditions.Count)
                    return SimpleConditionsMet(layerModel, dataModel, checkConditions);

                var conditionMet = false;
                switch (layerModel.Properties.ConditionType)
                {
                    case ConditionType.AnyMet:
                        conditionMet = HotKeyMet || checkConditions.Any(cm => cm.ConditionMet(dataModel));
                        break;
                    case ConditionType.AllMet:
                        conditionMet = HotKeyMet && checkConditions.All(cm => cm.ConditionMet(dataModel));
                        break;
                    case ConditionType.NoneMet:
                        conditionMet = !HotKeyMet && !checkConditions.Any(cm => cm.ConditionMet(dataModel));
                        break;
                }

                return conditionMet;
            }
        }

        private static bool SimpleConditionsMet(LayerModel layerModel, ModuleDataModel dataModel, IEnumerable<LayerConditionModel> checkConditions)
        {
            switch (layerModel.Properties.ConditionType)
            {
                case ConditionType.AnyMet:
                    return checkConditions.Any(cm => cm.ConditionMet(dataModel));
                case ConditionType.AllMet:
                    return checkConditions.All(cm => cm.ConditionMet(dataModel));
                case ConditionType.NoneMet:
                    return !checkConditions.Any(cm => cm.ConditionMet(dataModel));
                default:
                    return false;
            }
        }
    }
}
