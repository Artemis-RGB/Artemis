using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;

namespace Artemis.Modules.Games.WoW.Models
{
    [MoonSharpUserData]
    public class WoWSpecialization
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public string Role { get; set; }

        public void ApplyJson(JToken specJson)
        {
            Name = specJson["n"].Value<string>();
            Id = specJson["id"].Value<int>();
            Role = specJson["r"].Value<string>();
        }
    }
}
