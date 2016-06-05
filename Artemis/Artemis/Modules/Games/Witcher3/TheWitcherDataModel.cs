using Artemis.Models.Interfaces;

namespace Artemis.Modules.Games.Witcher3
{
    public class TheWitcherDataModel : IGameDataModel
    {
        public WitcherSign WitcherSign { get; set; }
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