using Artemis.Models.Interfaces;

namespace Artemis.Modules.Games.Witcher3
{
    public class Witcher3DataModel : IDataModel
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