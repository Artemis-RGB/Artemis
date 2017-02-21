using Artemis.Modules.Abstract;
using Artemis.Profiles.Layers.Models;
using Newtonsoft.Json;

namespace Artemis.Profiles.Layers.Abstract
{
    public abstract class LayerCondition
    {
        [JsonIgnore]
        public bool HotKeyMet { get; set; }

        public abstract bool ConditionsMet(LayerModel layerModel, ModuleDataModel dataModel);

        
        public void KeyDownTask(LayerConditionModel condition)
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

        public void KeyUpTask(LayerConditionModel condition)
        {
            if (condition.Field == "hotkeyEnable")
                HotKeyMet = false;
            else
                HotKeyMet = true;
        }
    }
}