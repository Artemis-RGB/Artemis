using Artemis.Models.Interfaces;
using Artemis.Models.Profiles;

namespace Artemis.Layers.Interfaces
{
    public interface ILayerCondition
    {
        bool ConditionsMet<T>(IDataModel dataModel, LayerModel layer);
    }
}