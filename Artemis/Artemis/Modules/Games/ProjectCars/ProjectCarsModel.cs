using Artemis.DAL;
using Artemis.Managers;
using Artemis.Modules.Abstract;
using Artemis.Modules.Games.ProjectCars.Data;

namespace Artemis.Modules.Games.ProjectCars
{
    public class ProjectCarsModel : ModuleModel
    {
        public ProjectCarsModel(DeviceManager deviceManager, LuaManager luaManager) : base(deviceManager, luaManager)
        {
            Settings = SettingsProvider.Load<ProjectCarsSettings>();
            DataModel = new ProjectCarsDataModel();
            ProcessNames.Add("pCARS64");
        }

        public override string Name => "ProjectCars";
        public override bool IsOverlay => false;
        public override bool IsBoundToProcess => true;

        public override void Update()
        {
            var dataModel = (ProjectCarsDataModel) DataModel;
            var returnTuple = pCarsAPI_GetData.ReadSharedMemoryData();

            // item1 is the true/false indicating a good read or not
            if (returnTuple.Item1)
                dataModel.GameData = dataModel.GameData.MapStructToClass(returnTuple.Item2, dataModel.GameData);
        }
    }
}