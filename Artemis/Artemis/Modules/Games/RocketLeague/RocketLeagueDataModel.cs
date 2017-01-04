using Artemis.Modules.Abstract;
using MoonSharp.Interpreter;

namespace Artemis.Modules.Games.RocketLeague
{
    [MoonSharpUserData]
    public class RocketLeagueDataModel : ModuleDataModel
    {
        public int Boost { get; set; }
    }
}