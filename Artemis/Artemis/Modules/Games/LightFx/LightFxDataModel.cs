using Artemis.Models.Interfaces;
using Artemis.Modules.Games.LightFx.Data;
using MoonSharp.Interpreter;
using Newtonsoft.Json;

namespace Artemis.Modules.Games.LightFx
{
    [MoonSharpUserData]
    public class LightFxDataModel : IDataModel
    {
        [JsonProperty(PropertyName = "lightFxState")]
        public LightFxState LightFxState { get; set; }

        public LightFxDataModel()
        {
            LightFxState = new LightFxState();
        }
    }
}