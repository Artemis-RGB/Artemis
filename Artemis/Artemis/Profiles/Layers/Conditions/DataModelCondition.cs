using System.Linq;
using Artemis.Modules.Abstract;
using Artemis.Profiles.Layers.Abstract;
using Artemis.Profiles.Layers.Models;

namespace Artemis.Profiles.Layers.Conditions
{
    public class DataModelCondition : ILayerCondition
    {
        public bool ConditionsMet(LayerModel layerModel, ModuleDataModel dataModel)
        {
            lock (layerModel.Properties.Conditions)
            {
                var checkConditions = layerModel.Properties.Conditions.Where(c => c.Field != null).ToList();
                switch (layerModel.Properties.ConditionType)
                {
                    case ConditionType.AnyMet:
                        return checkConditions.Any(cm => cm.ConditionMet(dataModel));
                    case ConditionType.AllMet:
                        return checkConditions.All(cm => cm.ConditionMet(dataModel));
                    case ConditionType.NoneMet:
                        return !checkConditions.Any(cm => cm.ConditionMet(dataModel));
                }
                return false;
            }
        }
    }
}
