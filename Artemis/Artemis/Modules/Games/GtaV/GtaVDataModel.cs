using Artemis.Models.Interfaces;
using MoonSharp.Interpreter;

namespace Artemis.Modules.Games.GtaV
{
    [MoonSharpUserData]
    public class GtaVDataModel : IDataModel
    {
        public bool IsWanted { get; set; }
        public string Color { get; set; }
    }
}