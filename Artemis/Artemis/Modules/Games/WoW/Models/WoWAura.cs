using System;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;

namespace Artemis.Modules.Games.WoW.Models
{
    [MoonSharpUserData]
    public class WoWAura
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public int Stacks { get; set; }
        public DateTime StartTime { set; get; }
        public DateTime EndTime { get; set; }
        public float Progress { get; set; }

        public void ApplyJson(JToken buffJson)
        {
            Name = buffJson["n"].Value<string>();
            Id = buffJson["id"].Value<int>();
            if (buffJson["c"] != null)
                Stacks = buffJson["c"].Value<int>();

            if (buffJson["e"] != null)
            {
                var expires = buffJson["e"].Value<int>();
                var tickCount = Environment.TickCount;
                var timeLeft = expires - tickCount;
                EndTime = DateTime.Now.AddMilliseconds(timeLeft);
            }
            if (buffJson["d"] != null)
                StartTime = EndTime.AddSeconds(buffJson["d"].Value<int>() * -1);
        }

        public void UpdateProgress()
        {
            var elapsed = DateTime.Now - StartTime;
            var total = EndTime - StartTime;

            Progress = (float) (elapsed.TotalMilliseconds / total.TotalMilliseconds);
            if (Progress > 1)
                Progress = 1;
        }
    }
}
