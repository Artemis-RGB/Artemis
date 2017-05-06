using Artemis.Modules.Abstract;
using MoonSharp.Interpreter;

namespace Artemis.Modules.Games.Terraria
{
    [MoonSharpUserData]
    public class TerrariaDataModel : ModuleDataModel
    {
        public int Hp { get; set; }
        public int MaxHp { get; set; }
        public int Mana { get; set; }
        public int MaxMana { get; set; }
        public int Breath { get; set; }
        public int MaxBreath { get; set; }
        public bool InWater { get; set; }
        public bool InLava { get; set; }
    }
}