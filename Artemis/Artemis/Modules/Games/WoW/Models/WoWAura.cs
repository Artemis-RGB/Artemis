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
        public string Caster { get; set; }
        public int Stacks { get; set; }
        public DateTime StartTime { set; get; }
        public DateTime EndTime { get; set; }

        public void ApplyJson(JToken buffJson)
        {
            Name = buffJson["name"].Value<string>();
            Id = buffJson["spellID"].Value<int>();
            Stacks = buffJson["count"].Value<int>();
            Caster = buffJson["caster"]?.Value<string>();

            // TODO: Duration
        }
    }
}
