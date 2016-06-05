using Artemis.Models.Interfaces;

namespace Artemis.Models.Profiles.Properties
{
    public class HeadsetPropertiesModel : LayerPropertiesModel
    {
        public override AppliedProperties GetAppliedProperties(IGameDataModel dataModel, bool ignoreDynamic = false)
        {
            return new AppliedProperties();
        }
    }
}