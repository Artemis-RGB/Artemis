using Artemis.Models.Interfaces;
using Artemis.Profiles.Layers.Models;

namespace Artemis.Profiles.Layers.Interfaces
{
    public interface ILayerCondition
    {
        bool ConditionsMet(LayerModel layerModel, IDataModel dataModel);
    }
}