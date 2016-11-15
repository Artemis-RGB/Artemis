using Artemis.Models.Interfaces;
using MoonSharp.Interpreter;

namespace Artemis.Modules.Games.RocketLeague
{
    [MoonSharpUserData]
    public class RocketLeagueDataModel : IDataModel
    {
        public int Boost { get; set; }
    }
}