using Artemis.DAL;
using Artemis.Managers;
using Artemis.Modules.Abstract;
using Artemis.Modules.General.GeneralProfile;
using Ninject;

namespace Artemis.Modules.Overlays.OverlayProfile
{
    public class OverlayProfileModel : ModuleModel
    {
        private readonly GeneralProfileModel _generalProfileModel;

        public OverlayProfileModel(DeviceManager deviceManager, LuaManager luaManager,
            [Named(nameof(GeneralProfileModel))] ModuleModel generalProfileModel) : base(deviceManager, luaManager)
        {
            _generalProfileModel = (GeneralProfileModel) generalProfileModel;
            Settings = SettingsProvider.Load<OverlayProfileSettings>();
            DataModel = new OverlayProfileDataModel();
        }

        public override string Name => "OverlayProfile";
        public override bool IsOverlay => true;
        public override bool IsBoundToProcess => false;

        public override void Update()
        {
            // TODO: Find a clean way to update the parent profile model
            DataModel.ParentDataModel = _generalProfileModel.DataModel;
        }
    }
}