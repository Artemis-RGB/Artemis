using System;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;

namespace Artemis.Modules.Games.WoW.Models
{
    [MoonSharpUserData]
    public class WoWCastBar
    {
        public WoWCastBar()
        {
            Spell = new WoWSpell();
        }

        public WoWSpell Spell { get; set; }
        public DateTime StartTime { set; get; }
        public DateTime EndTime { get; set; }
        public bool NonInterruptible { get; set; }
        public float Progress { get; set; }

        public void ApplyJson(JToken spellJson)
        {
            var castMs = spellJson["endTime"].Value<int>() - spellJson["startTime"].Value<int>();
            var tickCount = Environment.TickCount;
            var difference = tickCount - spellJson["startTime"].Value<int>();

            Spell.Name = spellJson["name"].Value<string>();
            Spell.Id = spellJson["spellID"].Value<int>();
            StartTime = new DateTime(DateTime.Now.Ticks + difference);
            EndTime = StartTime.AddMilliseconds(castMs);
            NonInterruptible = spellJson["notInterruptible"].Value<bool>();
        }

        public void UpdateProgress()
        {
            if (Spell.Name == null)
                return;

            var elapsed = DateTime.Now - StartTime;
            var total = EndTime - StartTime;

            Progress = (float) (elapsed.TotalMilliseconds / total.TotalMilliseconds);
            if (Progress > 1)
                Clear();
        }

        public void Clear()
        {
            Spell.Name = null;
            Spell.Id = 0;
            StartTime = DateTime.MinValue;
            EndTime = DateTime.MinValue;
            NonInterruptible = false;
            Progress = 0;
        }
    }
}
