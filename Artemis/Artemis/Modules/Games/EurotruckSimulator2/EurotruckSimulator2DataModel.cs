using Artemis.Models.Interfaces;
using Artemis.Modules.Games.EurotruckSimulator2.Data;
using MoonSharp.Interpreter;

namespace Artemis.Modules.Games.EurotruckSimulator2
{
    [MoonSharpUserData]
    public class EurotruckSimulator2DataModel : IDataModel
    {
        public EurotruckSimulator2DataModel()
        {
            // Register types for LUA
            UserData.RegisterType<IEts2Game>();
            UserData.RegisterType<IEts2Job>();
            UserData.RegisterType<IEts2Navigation>();
            UserData.RegisterType<IEts2Trailer>();
            UserData.RegisterType<IEts2Truck>();
        }

        public IEts2Game Game { get; set; }
        public IEts2Job Job { get; set; }
        public IEts2Navigation Navigation { get; set; }
        public IEts2Trailer Trailer { get; set; }
        public IEts2Truck Truck { get; set; }
    }
}