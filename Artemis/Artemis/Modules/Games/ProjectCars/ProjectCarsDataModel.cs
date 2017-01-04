using Artemis.Modules.Abstract;
using Artemis.Modules.Games.ProjectCars.Data;
using MoonSharp.Interpreter;

namespace Artemis.Modules.Games.ProjectCars
{
    [MoonSharpUserData]
    public class ProjectCarsDataModel : ModuleDataModel
    {
        public ProjectCarsDataModel()
        {
            GameData = new pCarsDataClass();
        }

        public pCarsDataClass GameData { get; set; }
    }
}