using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Artemis.Modules.Abstract;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Models;

namespace Artemis.Profiles.Layers.Conditions
{
    public class DataModelCondition : ILayerCondition
    {
        private DateTime _lastKeypress;
        public bool HotKeyMet { get; set; }
        private static readonly TimeSpan Delay = TimeSpan.FromMilliseconds((SystemParameters.KeyboardDelay + 1) * 250);

        public bool ConditionsMet(LayerModel layerModel, ModuleDataModel dataModel)
        {
            lock (layerModel.Properties.Conditions)
            {
                var checkConditions = layerModel.Properties.Conditions.Where(c => !c.Field.Contains("hotkey"));
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

                // If there is a held down keybind on it, reset every 2 frames, after 500 ms
                if (layerModel.Properties.Conditions.Any(c => c.Operator == "held") && DateTime.Now - _lastKeypress > Delay)
                    HotKeyMet = false;

                return conditionMet;
            }
        }


        public void KeybindTask(LayerConditionModel condition)
        {
            _lastKeypress = DateTime.Now;
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
    }
}