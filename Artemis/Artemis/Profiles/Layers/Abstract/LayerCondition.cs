using Artemis.Modules.Abstract;
using Artemis.Profiles.Layers.Models;

namespace Artemis.Profiles.Layers.Abstract
{
    public abstract class LayerCondition
    {
        public abstract bool ConditionsMet(LayerModel layerModel, ModuleDataModel dataModel);
        public abstract void KeybindTask(LayerConditionModel condition);
    }
}