using MoonSharp.Interpreter;

namespace Artemis.Modules.Games.WoW.Models
{
    [MoonSharpUserData]
    public class WoWSpell
    {
        public string Name { get; set; }
        public int Id { get; set; }
    }
}