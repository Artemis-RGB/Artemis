using Artemis.Modules.Abstract;
using MoonSharp.Interpreter;

namespace Artemis.Modules.Games.Witcher3
{
    [MoonSharpUserData]
    public class Witcher3DataModel : ModuleDataModel
    {
        public WitcherSign WitcherSign { get; set; }
        public int MaxHealth { get; set; }
        public int Health { get; set; }
        public int Stamina { get; set; }
        public int Toxicity { get; set; }
        public int Vitality { get; set; }
    }

    public enum WitcherSign
    {
        Aard,
        Yrden,
        Igni,
        Quen,
        Axii
    }
}