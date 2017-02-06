using Artemis.Modules.Abstract;
using Artemis.Profiles.Layers.Models;

namespace Artemis.Profiles.Layers.Interfaces
{
    public interface ILayerCondition
    {
        bool ConditionsMet(LayerModel layerModel, ModuleDataModel dataModel);
        void KeybindTask(LayerConditionModel condition);
    }
}