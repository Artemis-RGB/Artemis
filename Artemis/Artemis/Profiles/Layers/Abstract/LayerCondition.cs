using Artemis.Modules.Abstract;
using Artemis.Profiles.Layers.Models;

namespace Artemis.Profiles.Layers.Abstract
{
    public interface ILayerCondition
    {
        bool ConditionsMet(LayerModel layerModel, ModuleDataModel dataModel);
    }
}
