using Artemis.Modules.Abstract;
using Artemis.Modules.General.GeneralProfile;

namespace Artemis.Modules.Overlays.OverlayProfile
{
    public class OverlayProfileDataModel : ModuleDataModel
    {
        public OverlayProfileDataModel()
        {
            ParentDataModel = new GeneralProfileDataModel();
        }
    }
}