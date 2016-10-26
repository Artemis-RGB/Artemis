using Artemis.Models.Interfaces;
using Artemis.Modules.Games.EurotruckSimulator2.Data;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Artemis.Modules.Games.EurotruckSimulator2
{
    [MoonSharpUserData]
    public class EurotruckSimulator2DataModel : IDataModel
    {
        // TODO: Test LUA functionality
        [MoonSharpVisible(true)]
        public IEts2Game Game { get; set; }
        [MoonSharpVisible(true)]
        public IEts2Job Job { get; set; }
        [MoonSharpVisible(true)]
        public IEts2Navigation Navigation { get; set; }
        [MoonSharpVisible(true)]
        public IEts2Trailer Trailer { get; set; }
        [MoonSharpVisible(true)]
        public IEts2Truck Truck { get; set; }
    }
}