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
        public bool IsChannel { get; set; }

        public void ApplyJson(JToken spellJson, bool isChannel)
        {
            var castMs = spellJson["e"].Value<int>() - spellJson["s"].Value<int>();
            var tickCount = Environment.TickCount;
            var difference = tickCount - spellJson["s"].Value<int>();

            Spell.Name = spellJson["n"].Value<string>();
            Spell.Id = spellJson["sid"].Value<int>();
            StartTime = new DateTime(DateTime.Now.Ticks + difference);
            EndTime = StartTime.AddMilliseconds(castMs);
            NonInterruptible = spellJson["ni"].Value<bool>();
            IsChannel = isChannel;
        }

        public void UpdateProgress()
        {
            if (Spell.Name == null)
                return;

            var elapsed = DateTime.Now - StartTime;
            var total = EndTime - StartTime;

            if (IsChannel)
                Progress = 1 - (float) (elapsed.TotalMilliseconds / total.TotalMilliseconds);
            else
                Progress = (float) (elapsed.TotalMilliseconds / total.TotalMilliseconds);

            if (Progress > 1 || Progress < 0)
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
            IsChannel = false;
        }
    }
}
