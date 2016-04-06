using Artemis.Models.Interfaces;

namespace Artemis.Modules.Games.CounterStrike
{
    public class CounterStrikeDataModel : IGameDataModel
    {
        public Provider provider { get; set; }
        public Map map { get; set; }
        public Round round { get; set; }
        public Player player { get; set; }
        public Previously previously { get; set; }
    }

    public class Provider
    {
        public string name { get; set; }
        public int appid { get; set; }
        public int version { get; set; }
        public string steamid { get; set; }
        public int timestamp { get; set; }
    }

    public class TeamCt
    {
        public int score { get; set; }
    }

    public class TeamT
    {
        public int score { get; set; }
    }

    public class Map
    {
        public string mode { get; set; }
        public string name { get; set; }
        public string phase { get; set; }
        public int round { get; set; }
        public TeamCt team_ct { get; set; }
        public TeamT team_t { get; set; }
    }

    public class Round
    {
        public string phase { get; set; }
    }

    public class State
    {
        public int health { get; set; }
        public int armor { get; set; }
        public bool helmet { get; set; }
        public int flashed { get; set; }
        public int smoked { get; set; }
        public int burning { get; set; }
        public int money { get; set; }
        public int round_kills { get; set; }
        public int round_killhs { get; set; }
    }

    public class Weapon0
    {
        public string name { get; set; }
        public string paintkit { get; set; }
        public string type { get; set; }
        public string state { get; set; }
    }

    public class Weapon1
    {
        public string name { get; set; }
        public string paintkit { get; set; }
        public string type { get; set; }
        public int ammo_clip { get; set; }
        public int ammo_clip_max { get; set; }
        public int ammo_reserve { get; set; }
        public string state { get; set; }
    }

    public class Weapon2
    {
        public string name { get; set; }
        public string paintkit { get; set; }
        public string type { get; set; }
        public string state { get; set; }
    }

    public class Weapons
    {
        public Weapon0 weapon_0 { get; set; }
        public Weapon1 weapon_1 { get; set; }
        public Weapon2 weapon_2 { get; set; }
    }

    public class MatchStats
    {
        public int kills { get; set; }
        public int assists { get; set; }
        public int deaths { get; set; }
        public int mvps { get; set; }
        public int score { get; set; }
    }

    public class Player
    {
        public string steamid { get; set; }
        public string name { get; set; }
        public string team { get; set; }
        public string activity { get; set; }
        public State state { get; set; }
        public Weapons weapons { get; set; }
        public MatchStats match_stats { get; set; }
    }

    public class Round2
    {
        public string phase { get; set; }
    }

    public class Previously
    {
        public Round2 round { get; set; }
    }
}