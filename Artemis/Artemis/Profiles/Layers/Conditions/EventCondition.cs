using System.Collections.Generic;
using System.Linq;
using Artemis.Modules.Abstract;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Models;
using Newtonsoft.Json;

namespace Artemis.Profiles.Layers.Conditions
{
    public class EventCondition : ILayerCondition
    {
        [JsonIgnore]
        public bool HotKeyMet { get; set; }

        public bool ConditionsMet(LayerModel layerModel, ModuleDataModel dataModel)
        {
            lock (layerModel.Properties.Conditions)
            {
                var checkConditions = layerModel.Properties.Conditions.Where(c => !c.Field.Contains("hotkey")).ToList();
                if (checkConditions.Count == layerModel.Properties.Conditions.Count)
                    return SimpleConditionsMet(layerModel, dataModel, checkConditions);

                var conditionsMet = false;
                switch (layerModel.Properties.ConditionType)
                {
                    case ConditionType.AnyMet:
                        conditionsMet = HotKeyMet || checkConditions.Any(cm => cm.ConditionMet(dataModel));
                        break;
                    case ConditionType.AllMet:
                        conditionsMet = HotKeyMet && checkConditions.All(cm => cm.ConditionMet(dataModel));
                        break;
                    case ConditionType.NoneMet:
                        conditionsMet = !HotKeyMet && !checkConditions.Any(cm => cm.ConditionMet(dataModel));
                        break;
                }

                layerModel.EventProperties.Update(layerModel, conditionsMet);

                if (conditionsMet)
                    layerModel.EventProperties.TriggerEvent(layerModel);
                if (layerModel.EventProperties.MustStop(layerModel))
                    HotKeyMet = false;

                return layerModel.EventProperties.MustDraw;
            }
        }

        public void KeybindTask(LayerConditionModel condition)
        {
            switch (condition.Field)
            {
                case "hotkeyEnable":
                    HotKeyMet = true;
                    break;
                case "hotkeyDisable":
                    HotKeyMet = false;
                    break;
                case "hotkeyToggle":
                    HotKeyMet = !HotKeyMet;
                    break;
            }
        }

        private static bool SimpleConditionsMet(LayerModel layerModel, ModuleDataModel dataModel,
            IEnumerable<LayerConditionModel> checkConditions)
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