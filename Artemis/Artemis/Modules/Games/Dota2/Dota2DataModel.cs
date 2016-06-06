using Artemis.Models.Interfaces;

namespace Artemis.Modules.Games.Dota2
{
    public class Dota2DataModel : IDataModel
    {
        public Provider provider { get; set; }
        public Map map { get; set; }
        public Player player { get; set; }
        public Hero hero { get; set; }
        public Abilities abilities { get; set; }
        public Items items { get; set; }
        public Previously previously { get; set; }
    }

    public class Provider
    {
        public string name { get; set; }
        public int appid { get; set; }
        public int version { get; set; }
        public int timestamp { get; set; }
    }

    public class Map
    {
        public int dayCyclePercentage;
        public string name { get; set; }
        public long matchid { get; set; }
        public int game_time { get; set; }
        public int clock_time { get; set; }
        public bool daytime { get; set; }
        public bool nightstalker_night { get; set; }
        public string game_state { get; set; }
        public string win_team { get; set; }
        public string customgamename { get; set; }
        public int ward_purchase_cooldown { get; set; }
    }

    public class Player
    {
        public string steamid { get; set; }
        public string name { get; set; }
        public string activity { get; set; }
        public int kills { get; set; }
        public int deaths { get; set; }
        public int assists { get; set; }
        public int last_hits { get; set; }
        public int denies { get; set; }
        public int kill_streak { get; set; }
        public string team_name { get; set; }
        public int gold { get; set; }
        public int gold_reliable { get; set; }
        public int gold_unreliable { get; set; }
        public int gpm { get; set; }
        public int xpm { get; set; }
    }

    public class Hero
    {
        public int id { get; set; }
        public string name { get; set; }
        public int level { get; set; }
        public bool alive { get; set; }
        public int respawn_seconds { get; set; }
        public int buyback_cost { get; set; }
        public int buyback_cooldown { get; set; }
        public int health { get; set; }
        public int max_health { get; set; }
        public int health_percent { get; set; }
        public int mana { get; set; }
        public int max_mana { get; set; }
        public int mana_percent { get; set; }
        public bool silenced { get; set; }
        public bool stunned { get; set; }
        public bool disarmed { get; set; }
        public bool magicimmune { get; set; }
        public bool hexed { get; set; }
        public bool muted { get; set; }
        public bool _break { get; set; }
        public bool has_debuff { get; set; }
    }

    public class Abilities
    {
        public Ability0 ability0 { get; set; }
        public Ability1 ability1 { get; set; }
        public Ability2 ability2 { get; set; }
        public Ability3 ability3 { get; set; }

        public Attributes attributes { get; set; }
    }

    public class Ability0
    {
        public string name { get; set; }
        public int level { get; set; }
        public bool can_cast { get; set; }
        public bool passive { get; set; }
        public bool ability_active { get; set; }
        public int cooldown { get; set; }
        public bool ultimate { get; set; }
    }

    public class Ability1
    {
        public string name { get; set; }
        public int level { get; set; }
        public bool can_cast { get; set; }
        public bool passive { get; set; }
        public bool ability_active { get; set; }
        public int cooldown { get; set; }
        public bool ultimate { get; set; }
    }

    public class Ability2
    {
        public string name { get; set; }
        public int level { get; set; }
        public bool can_cast { get; set; }
        public bool passive { get; set; }
        public bool ability_active { get; set; }
        public int cooldown { get; set; }
        public bool ultimate { get; set; }
    }

    public class Ability3
    {
        public string name { get; set; }
        public int level { get; set; }
        public bool can_cast { get; set; }
        public bool passive { get; set; }
        public bool ability_active { get; set; }
        public int cooldown { get; set; }
        public bool ultimate { get; set; }
    }

    public class Attributes
    {
        public int level { get; set; }
    }

    public class Items
    {
        public Slot0 slot0 { get; set; }
        public Slot1 slot1 { get; set; }
        public Slot2 slot2 { get; set; }
        public Slot3 slot3 { get; set; }
        public Slot4 slot4 { get; set; }
        public Slot5 slot5 { get; set; }
        public Stash0 stash0 { get; set; }
        public Stash1 stash1 { get; set; }
        public Stash2 stash2 { get; set; }
        public Stash3 stash3 { get; set; }
        public Stash4 stash4 { get; set; }
        public Stash5 stash5 { get; set; }
    }

    public class Slot0
    {
        public string name { get; set; }
    }

    public class Slot1
    {
        public string name { get; set; }
    }

    public class Slot2
    {
        public string name { get; set; }
    }

    public class Slot3
    {
        public string name { get; set; }
    }

    public class Slot4
    {
        public string name { get; set; }
    }

    public class Slot5
    {
        public string name { get; set; }
    }

    public class Stash0
    {
        public string name { get; set; }
    }

    public class Stash1
    {
        public string name { get; set; }
    }

    public class Stash2
    {
        public string name { get; set; }
    }

    public class Stash3
    {
        public string name { get; set; }
    }

    public class Stash4
    {
        public string name { get; set; }
    }

    public class Stash5
    {
        public string name { get; set; }
    }

    public class Previously
    {
        public Player1 player { get; set; }
    }

    public class Player1
    {
        public int gold { get; set; }
        public int gold_unreliable { get; set; }
    }
}