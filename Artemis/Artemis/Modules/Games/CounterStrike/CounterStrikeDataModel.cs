using Artemis.Modules.Abstract;
using MoonSharp.Interpreter;
using Newtonsoft.Json;

namespace Artemis.Modules.Games.CounterStrike
{
    [MoonSharpUserData]
    public class CounterStrikeDataModel : ModuleDataModel
    {
        public Provider provider { get; set; }
        public Map map { get; set; }
        public Round round { get; set; }
        public Player player { get; set; }
        public Previously previously { get; set; }
    }

    [MoonSharpUserData]
    public class Provider
    {
        public string name { get; set; }
        public int appid { get; set; }
        public int version { get; set; }
        public string steamid { get; set; }
        public int timestamp { get; set; }
    }

    [MoonSharpUserData]
    public class TeamCt
    {
        public int score { get; set; }
    }

    [MoonSharpUserData]
    public class TeamT
    {
        public int score { get; set; }
    }

    [MoonSharpUserData]
    public class Map
    {
        public string mode { get; set; }
        public string name { get; set; }
        public string phase { get; set; }
        public int round { get; set; }
        public TeamCt team_ct { get; set; }
        public TeamT team_t { get; set; }
    }

    [MoonSharpUserData]
    public class Round
    {
        public string phase { get; set; }
        public string bomb { get; set; }
        public string win_team { get; set; }
    }

    [MoonSharpUserData]
    public class State
    {
        [JsonIgnore]
        public bool made_kill { get; set; }
        [JsonIgnore]
        public bool made_headshot { get; set; }
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

    [MoonSharpUserData]
    public class Weapon
    {
        public string name { get; set; }
        public string paintkit { get; set; }
        public string type { get; set; }
        public int ammo_clip { get; set; }
        public int ammo_clip_max { get; set; }
        public int ammo_reserve { get; set; }
        public string state { get; set; }
    }

    [MoonSharpUserData]
    public class Weapons
    {
        public Weapon active_weapon { get; set; }
        public Weapon weapon_0 { get; set; }
        public Weapon weapon_1 { get; set; }
        public Weapon weapon_2 { get; set; }
    }

    [MoonSharpUserData]
    public class MatchStats
    {
        public int kills { get; set; }
        public int assists { get; set; }
        public int deaths { get; set; }
        public int mvps { get; set; }
        public int score { get; set; }
    }

    [MoonSharpUserData]
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

    [MoonSharpUserData]
    public class Round2
    {
        public string phase { get; set; }
    }

    [MoonSharpUserData]
    public class Previously
    {
        public Round2 round { get; set; }
    }
}