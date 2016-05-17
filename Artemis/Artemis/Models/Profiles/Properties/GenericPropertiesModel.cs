using Artemis.Models.Interfaces;
using Artemis.Utilities;

namespace Artemis.Models.Profiles.Properties
{
    public class GenericPropertiesModel : LayerPropertiesModel
    {
        public override LayerPropertiesModel GetAppliedProperties(IGameDataModel dataModel)
        {
            return GeneralHelpers.Clone(this); 
        }
    }
}