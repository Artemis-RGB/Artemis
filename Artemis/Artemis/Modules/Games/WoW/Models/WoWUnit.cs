using System.Collections.Generic;
using System.Linq;
using Artemis.Utilities;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;

namespace Artemis.Modules.Games.WoW.Models
{
    [MoonSharpUserData]
    public class WoWUnit
    {
        private readonly List<WoWSpell> _currentFrameCasts = new List<WoWSpell>();

        public WoWUnit()
        {
            CastBar = new WoWCastBar();
            Specialization = new WoWSpecialization();

            Buffs = new List<WoWAura>();
            Debuffs = new List<WoWAura>();
            RecentIntantCasts = new List<WoWSpell>();
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

        public WoWCastBar CastBar { get; set; }
        public WoWSpecialization Specialization { get; }

        public List<WoWAura> Buffs { get; }
        public List<WoWAura> Debuffs { get; }
        public List<WoWSpell> RecentIntantCasts { get; private set; }

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
            if (json["specialization"] != null)
                Specialization.ApplyJson(json["specialization"]);
        }

        public void ApplyStateJson(JObject json)
        {
            Health = json["health"].Value<int>();
            MaxHealth = json["maxHealth"].Value<int>();
            PowerType = GeneralHelpers.ParseEnum<WoWPowerType>(json["powerType"].Value<int>().ToString());
            Power = json["power"].Value<int>();
            MaxPower = json["maxPower"].Value<int>();
        }

        public void ApplyAuraJson(JObject json)
        {
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

        public void AddInstantCast(WoWSpell spell)
        {
            lock (_currentFrameCasts)
            {
                _currentFrameCasts.Add(spell);
            }
        }

        public void ClearInstantCasts()
        {
            lock (_currentFrameCasts)
            {
                // Remove all casts that weren't cast in the after the last frame
                RecentIntantCasts.Clear();
                RecentIntantCasts.AddRange(_currentFrameCasts);
                
                // Clear the that were after the last frame so that they are removed next frame when this method is called again
                _currentFrameCasts.Clear();
            }
        }

        public void Update()
        {
            CastBar.UpdateProgress();
            ClearInstantCasts();
        }
    }
}
