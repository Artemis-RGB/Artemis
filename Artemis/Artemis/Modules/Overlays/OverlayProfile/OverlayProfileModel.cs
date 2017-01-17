using Artemis.DAL;
using Artemis.Managers;
using Artemis.Modules.Abstract;
using Artemis.Modules.General.GeneralProfile;
using Ninject;

namespace Artemis.Modules.Overlays.OverlayProfile
{
    public class OverlayProfileModel : ModuleModel
    {
        private readonly GeneralProfileModel _generalProfileModule;

        public OverlayProfileModel(DeviceManager deviceManager, LuaManager luaManager,
            [Named(nameof(GeneralProfileModel))] ModuleModel generalProfileModule) : base(deviceManager, luaManager)
        {
            _generalProfileModule = (GeneralProfileModel) generalProfileModule;
            Settings = SettingsProvider.Load<OverlayProfileSettings>();
            DataModel = new OverlayProfileDataModel();
        }

        public override string Name => "OverlayProfile";
        public override bool IsOverlay => true;
        public override bool IsBoundToProcess => false;

        public override void Update()
        {
            var dataModel = (OverlayProfileDataModel) DataModel;

            if (!_generalProfileModule.IsInitialized)
                _generalProfileModule.Update();
            dataModel.GeneralDataModel = (GeneralProfileDataModel) _generalProfileModule.DataModel;
        }
    }
}