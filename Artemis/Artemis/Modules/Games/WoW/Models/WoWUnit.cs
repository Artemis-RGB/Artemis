using System.Collections.Generic;
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
        public List<WoWSpell> RecentIntantCasts { get; }

        public void ApplyJson(JToken json)
        {
            if (json["n"] == null)
                return;

            Name = json["n"].Value<string>();
            Level = json["l"].Value<int>();
            Class = json["c"].Value<string>();
            Gender = json["g"].Value<int>() == 3 ? WoWGender.Female : WoWGender.Male;

            if (json["r"] != null)
                Race = GeneralHelpers.ParseEnum<WoWRace>(json["r"].Value<string>());
            if (json["s"] != null)
                Specialization.ApplyJson(json["s"]);
        }

        public void ApplyStateJson(JToken json)
        {
            Health = json["h"].Value<int>();
            MaxHealth = json["mh"].Value<int>();
            PowerType = GeneralHelpers.ParseEnum<WoWPowerType>(json["t"].Value<int>().ToString());
            Power = json["p"].Value<int>();
            MaxPower = json["mp"].Value<int>();
        }

        public void ApplyAuraJson(JToken json, bool buffs)
        {
            if (buffs)
            {
                lock (Buffs)
                {
                    Buffs.Clear();
                    foreach (var auraJson in json.Children())
                    {
                        var aura = new WoWAura();
                        aura.ApplyJson(auraJson);
                        Buffs.Add(aura);
                    }
                }
            }
            else
            {
                lock (Debuffs)
                {
                    Debuffs.Clear();
                    foreach (var auraJson in json.Children())
                    {
                        var aura = new WoWAura();
                        aura.ApplyJson(auraJson);
                        Debuffs.Add(aura);
                    }
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

            lock (Buffs)
            {
                foreach (var buff in Buffs)
                    buff.UpdateProgress();
            }
            lock (Debuffs)
            {
                foreach (var debuff in Debuffs)
                    debuff.UpdateProgress();
            }
        }
    }
}
