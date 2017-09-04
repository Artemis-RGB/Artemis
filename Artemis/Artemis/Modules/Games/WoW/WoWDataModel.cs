using System;
using System.Collections.Generic;
using Artemis.Modules.Abstract;
using Castle.Components.DictionaryAdapter;
using Newtonsoft.Json.Linq;

namespace Artemis.Modules.Games.WoW
{
    public class WoWDataModel : ModuleDataModel
    {
        public WoWDataModel()
        {
            Player = new WoWUnit();
            Target = new WoWUnit();
        }

        public WoWUnit Player { get; set; }
        public WoWUnit Target { get; set; }
        public string Realm { get; set; }
        public string Zone { get; set; }
        public string SubZone { get; set; }
    }

    public class WoWUnit
    {
        public WoWUnit()
        {
            Buffs = new List<WoWAura>();
            Debuffs = new EditableList<WoWAura>();
        }

        public string Name { get; set; }
        public int Level { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int Power { get; set; }
        public int MaxPower { get; set; }
        public WoWPowerType PowerType { get; set; }
        public WoWClass Class { get; set; }
        public WoWRace Race { get; set; }
        public WoWGender Gender { get; set; }
        public List<WoWAura> Buffs { get; set; }
        public List<WoWAura> Debuffs { get; set; }

        public void ApplyJson(JObject json)
        {
            if (json["name"] == null)
                return;

            Name = json["name"].Value<string>();
            Level = json["level"].Value<int>();
            Class = (WoWClass) Enum.Parse(typeof(WoWClass), json["class"].Value<string>().Replace(" ", ""));
            Race = (WoWRace) Enum.Parse(typeof(WoWRace), json["race"].Value<string>().Replace(" ", ""));
            Gender = json["gender"].Value<int>() == 3 ? WoWGender.Female : WoWGender.Male;
        }

        public void ApplyStateJson(JObject json)
        {
            Health = json["health"].Value<int>();
            MaxHealth = json["maxHealth"].Value<int>();
            PowerType = (WoWPowerType) Enum.Parse(typeof(WoWPowerType), json["powerType"].Value<int>().ToString(), true);
            Power = json["power"].Value<int>();
            MaxPower = json["maxPower"].Value<int>();

            Buffs.Clear();
            foreach (var auraJson in json["buffs"].Children())
            {
                var aura = new WoWAura();
                aura.ApplyJson(auraJson);
                Buffs.Add(aura);
            }
            Debuffs.Clear();
            foreach (var auraJson in json["debuffs"].Children())
            {
                var aura = new WoWAura();
                aura.ApplyJson(auraJson);
                Debuffs.Add(aura);
            }
        }
    }

    public class WoWAura
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public string Caster { get; set; }
        public int Stacks { get; set; }
        public TimeSpan Duration { get; set; }
        public TimeSpan Expires { get; set; }

        public void ApplyJson(JToken buffJson)
        {
            Name = buffJson["name"].Value<string>();
            Id = buffJson["spellID"].Value<int>();
            Caster = buffJson["caster"].Value<string>();
            Stacks = buffJson["count"].Value<int>();

            // TODO: Duration
        }
    }

    public enum WoWPowerType
    {
        Mana = 0,
        Rage = 1,
        Focus = 2,
        Energy = 3,
        ComboPoints = 4,
        Runes = 5,
        RunicPower = 6,
        SoulShards = 7,
        LunarPower = 8,
        HolyPower = 9,
        AlternatePower = 10,
        Maelstrom = 11,
        Chi = 12,
        Insanity = 13,
        ArcaneCharges = 16
    }

    public enum WoWClass
    {
        Warrior,
        Paladin,
        Hunter,
        Rogue,
        Priest,
        DeathKnight,
        Shaman,
        Mage,
        Warlock,
        Druid,
        Monk,
        DemonHunter
    }

    public enum WoWRace
    {
        Human,
        Orc,
        Dwarf,
        NightElf,
        Undead,
        Tauren,
        Gnome,
        Troll,
        BloodElf,
        Draenei,
        Goblin,
        Worgen,
        Pandaren
    }

    public enum WoWGender
    {
        Male,
        Female
    }
}
