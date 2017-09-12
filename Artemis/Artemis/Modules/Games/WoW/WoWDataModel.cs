using Artemis.Modules.Abstract;
using Artemis.Modules.Games.WoW.Models;
using MoonSharp.Interpreter;

namespace Artemis.Modules.Games.WoW
{
    [MoonSharpUserData]
    public class WoWDataModel : ModuleDataModel
    {
        public WoWDataModel()
        {
            Player = new WoWUnit();
            Target = new WoWUnit();
        }

        public WoWUnit Player { get; set; }
        public WoWUnit Target { get; set; }

        public string Realm { get; set; }
        public string Zone { get; set; }
        public string SubZone { get; set; }

        public WoWState State { get; set; }
    }

    public enum WoWState
    {
        LoggedOut,
        Ingame,
        Afk,
        Dnd
    }
}
