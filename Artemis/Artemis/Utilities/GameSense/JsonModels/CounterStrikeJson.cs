using System.Collections.Generic;

namespace Artemis.Utilities.GameSense.JsonModels
{
    public class CounterStrikeJson
    {
        public Provider provider { get; set; }
        public Map map { get; set; }
        public Round round { get; set; }
        public Player player { get; set; }
        public Auth auth { get; set; }
    }

    public class Provider
    {
        public string name { get; set; }
        public int appid { get; set; }
        public int version { get; set; }
        public string steamid { get; set; }
        public int timestamp { get; set; }
    }

    public class Map
    {
        public string name { get; set; }
        public string phase { get; set; }
        public int round { get; set; }
        public Team_Ct team_ct { get; set; }
        public Team_T team_t { get; set; }
    }

    public class Team_Ct
    {
        public int score { get; set; }
    }

    public class Team_T
    {
        public int score { get; set; }
    }

    public class Round
    {
        public string phase { get; set; }
    }

    public class Player
    {
        public string steamid { get; set; }
        public string name { get; set; }
        public string team { get; set; }
        public string activity { get; set; }
        public State state { get; set; }
        public Dictionary<string, Weapon> weapons { get; set; }
        public Match_Stats match_stats { get; set; }
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

    public class Match_Stats
    {
        public int kills { get; set; }
        public int assists { get; set; }
        public int deaths { get; set; }
        public int mvps { get; set; }
        public int score { get; set; }
    }

    public class Weapon
    {
        public string name { get; set; }
        public string paintkit { get; set; }
        public string type { get; set; }
        public string state { get; set; }
        public int ammo_clip { get; set; }
        public int ammo_clip_max { get; set; }
        public int ammo_reserve { get; set; }
    }

    public class Auth
    {
        public string key1 { get; set; }
        public string key2 { get; set; }
    }
}