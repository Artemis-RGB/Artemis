using Artemis.Models.Interfaces;
using Artemis.Modules.Games.ProjectCars.Data;
using MoonSharp.Interpreter;

namespace Artemis.Modules.Games.ProjectCars
{
    [MoonSharpUserData]
    public class ProjectCarsDataModel : IDataModel
    {
        public ProjectCarsDataModel()
        {
            GameData = new pCarsDataClass();
        }

        public pCarsDataClass GameData { get; set; }
    }
}