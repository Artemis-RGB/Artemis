using Artemis.Models.Interfaces;
using MoonSharp.Interpreter;

namespace Artemis.Modules.Games.TheDivision
{
    [MoonSharpUserData]
    public class TheDivisionDataModel : IDataModel
    {
        public PlayerState PartyMember1 { get; set; }
        public PlayerState PartyMember2 { get; set; }
        public PlayerState PartyMember3 { get; set; }

        public bool LowAmmo { get; set; }
        public bool LowHp { get; set; }
        public GrenadeState GrenadeState { get; set; }
    }
    
    public enum GrenadeState
    {
        HasGrenade,
        HasNoGrenade,
        GrenadeEquipped
    }

    public enum PlayerState
    {
        Offline,
        Online,
        Hit
    }
}