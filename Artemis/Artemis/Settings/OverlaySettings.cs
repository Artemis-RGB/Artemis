using System.ComponentModel;
using Newtonsoft.Json;

namespace Artemis.Settings
{
    public class OverlaySettings : EffectSettings
    {
        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool Enabled { get; set; }
    }
}