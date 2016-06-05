using Artemis.Models.Interfaces;
using Artemis.Utilities;

namespace Artemis.Models.Profiles.Properties
{
    public class FolderPropertiesModel : LayerPropertiesModel
    {
        public override AppliedProperties GetAppliedProperties(IGameDataModel dataModel, bool ignoreDynamic = false)
        {
            return new AppliedProperties();
        }
    }
}