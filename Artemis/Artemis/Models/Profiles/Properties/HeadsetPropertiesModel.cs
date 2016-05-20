using Artemis.Models.Interfaces;
using Artemis.Utilities;

namespace Artemis.Models.Profiles.Properties
{
    public class HeadsetPropertiesModel : LayerPropertiesModel
    {
        public override LayerPropertiesModel GetAppliedProperties(IGameDataModel dataModel)
        {
            // TODO: Apply any properties, if applicable to headsets in the first place.
            return GeneralHelpers.Clone(this);
        }
    }
}