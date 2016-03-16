using System.Collections.Generic;

namespace Artemis.Modules.Games.TheDivision
{
    public class TheDivisionDataModel
    {
        public List<DivisionPlayer> DivisionPlayers { get; set; }
        public GrenadeState GrenadeState { get; set; }
        public bool LowAmmo { get; set; }
        public bool LowHp { get; set; }

        public TheDivisionDataModel()
        {
            DivisionPlayers = new List<DivisionPlayer>();
        }
    }

    public class DivisionPlayer
    {
        public int Id { get; set; }
        public PlayerState PlayerState { get; set; }

        public DivisionPlayer(int id)
        {
            Id = id;
        }
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