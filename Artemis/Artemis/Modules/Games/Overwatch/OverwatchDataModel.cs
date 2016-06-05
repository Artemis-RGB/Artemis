using Artemis.Models.Interfaces;

namespace Artemis.Modules.Games.Overwatch
{
    public class OverwatchDataModel : IGameDataModel
    {
        public OverwatchStatus Status { get; set; }
        public OverwatchCharacter Character { get; set; }
        public bool UltimateReady { get; set; }
        public bool Ability1Ready { get; set; }
        public bool Ability2Ready { get; set; }
    }

    public enum OverwatchStatus
    {
        Unkown,
        InMainMenu,
        InGame
    }

    public enum OverwatchCharacter
    {
        None,
        Genji,
        Mccree,
        Pharah,
        Reaper,
        Soldier76,
        Tracer,
        Bastion,
        Hanzo,
        Junkrat,
        Mei,
        Torbjörn,
        Widowmaker,
        Dva,
        Reinhardt,
        Roadhog,
        Winston,
        Zarya,
        Lúcio,
        Mercy,
        Symmetra,
        Zenyatta
    }
}