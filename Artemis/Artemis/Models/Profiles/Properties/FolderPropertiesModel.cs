using Artemis.Models.Interfaces;

namespace Artemis.Models.Profiles.Properties
{
    public class FolderPropertiesModel : LayerPropertiesModel
    {
        public override AppliedProperties GetAppliedProperties(IDataModel dataModel, bool ignoreDynamic = false)
        {
            return new AppliedProperties();
        }
    }
}