using Artemis.Modules.Abstract;
using MoonSharp.Interpreter;

namespace Artemis.Modules.Games.GtaV
{
    [MoonSharpUserData]
    public class GtaVDataModel : ModuleDataModel
    {
        public bool IsWanted { get; set; }
        public string Color { get; set; }
    }
}