using Artemis.Models.Interfaces;
using MoonSharp.Interpreter;

namespace Artemis.Modules.Games.Overwatch
{
    [MoonSharpUserData]
    public class OverwatchDataModel : IDataModel
    {
        public OverwatchStatus Status { get; set; }
        public OverwatchCharacter Character { get; set; }
        public bool UltimateReady { get; set; }
        public bool Ability1Ready { get; set; }
        public bool Ability2Ready { get; set; }
        public bool UltimateUsed { get; set; }
        public bool CanChangeHero { get; set; }
    }

    public enum OverwatchStatus
    {
        Unknown,
        InMainMenu,
        InGame,
        InCharacterSelect
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
        Zenyatta,
        Ana,
        Sombra
    }
}