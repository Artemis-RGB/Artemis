using System;
using System.Collections.Generic;
using System.Diagnostics;
using Artemis.Modules.Abstract;
using Artemis.Utilities;
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
            Debuffs = new List<WoWAura>();
            CastBar = new WoWCastBar();
        }

        public string Name { get; set; }
        public int Level { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int Power { get; set; }
        public int MaxPower { get; set; }
        public WoWPowerType PowerType { get; set; }
        public string Class { get; set; }
        public WoWRace Race { get; set; }
        public WoWGender Gender { get; set; }
        public List<WoWAura> Buffs { get; set; }
        public List<WoWAura> Debuffs { get; set; }
        public WoWCastBar CastBar { get; set; }

        public void ApplyJson(JObject json)
        {
            if (json["name"] == null)
                return;

            Name = json["name"].Value<string>();
            Level = json["level"].Value<int>();
            Class = json["class"].Value<string>();
            Gender = json["gender"].Value<int>() == 3 ? WoWGender.Female : WoWGender.Male;

            if (json["race"] != null)
                Race = GeneralHelpers.ParseEnum<WoWRace>(json["race"].Value<string>());
        }

        public void ApplyStateJson(JObject json)
        {
            Health = json["health"].Value<int>();
            MaxHealth = json["maxHealth"].Value<int>();
            PowerType = GeneralHelpers.ParseEnum<WoWPowerType>(json["powerType"].Value<int>().ToString());
            Power = json["power"].Value<int>();
            MaxPower = json["maxPower"].Value<int>();

            Buffs.Clear();
            if (json["buffs"] != null)
            {
                foreach (var auraJson in json["buffs"].Children())
                {
                    var aura = new WoWAura();
                    aura.ApplyJson(auraJson);
                    Buffs.Add(aura);
                }
            }
            Debuffs.Clear();
            if (json["debuffs"] != null)
            {
                foreach (var auraJson in json["debuffs"].Children())
                {
                    var aura = new WoWAura();
                    aura.ApplyJson(auraJson);
                    Debuffs.Add(aura);
                }
            }
        }
    }

    public class WoWAura
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public string Caster { get; set; }
        public int Stacks { get; set; }
        public DateTime StartTime { set; get; }
        public DateTime EndTime { get; set; }

        public void ApplyJson(JToken buffJson)
        {
            Name = buffJson["name"].Value<string>();
            Id = buffJson["spellID"].Value<int>();
            Caster = buffJson["caster"].Value<string>();
            Stacks = buffJson["count"].Value<int>();

            // TODO: Duration
        }
    }

    public class WoWCastBar
    {
        public void ApplyJson(JToken spellJson)
        {
            var castMs = spellJson["endTime"].Value<int>() - spellJson["startTime"].Value<int>();
            var tickCount = Environment.TickCount;
            var difference = tickCount - spellJson["startTime"].Value<int>();

            SpellName = spellJson["name"].Value<string>();
            SpellId = spellJson["spellID"].Value<int>();
            StartTime = new DateTime(DateTime.Now.Ticks + difference);
            EndTime = StartTime.AddMilliseconds(castMs);
            NonInterruptible = spellJson["notInterruptible"].Value<bool>();


//            SpellName = spellJson["name"].Value<string>();
//            SpellId = spellJson["spellID"].Value<int>();
//            StartTime = DateTime.Now.AddMilliseconds(spellJson["startTime"].Value<long>()/1000.0);
//            EndTime = StartTime.AddMilliseconds(spellJson["endTime"].Value<long>()/1000.0);
//            NonInterruptible = spellJson["notInterruptible"].Value<bool>();
        }

        public void UpdateProgress()
        {
            if (SpellName == null)
                return;

            var elapsed = DateTime.Now - StartTime;
            var total = EndTime - StartTime;
            Progress = (float) (elapsed.TotalMilliseconds / total.TotalMilliseconds);
            Debug.WriteLine(Progress);
            if (Progress > 1)
                Clear();
        }

        public void Clear()
        {
            SpellName = null;
            SpellId = 0;
            StartTime = DateTime.MinValue;
            EndTime = DateTime.MinValue;
            NonInterruptible = false;
            Progress = 0;
        }

        public string SpellName { get; set; }
        public int SpellId { get; set; }
        public DateTime StartTime { set; get; }
        public DateTime EndTime { get; set; }
        public bool NonInterruptible { get; set; }
        public float Progress { get; set; }
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
