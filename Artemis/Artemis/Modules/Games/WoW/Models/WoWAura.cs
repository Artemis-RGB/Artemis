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

        public void ApplyJson(JToken buffJson)
        {
            Name = buffJson["n"].Value<string>();
            Id = buffJson["id"].Value<int>();
            if (buffJson["c"] != null)
                Stacks = buffJson["c"].Value<int>();

            // TODO: Duration
        }
    }
}
