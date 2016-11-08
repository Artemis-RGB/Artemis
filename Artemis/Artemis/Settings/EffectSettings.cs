using System.ComponentModel;
using Artemis.DAL;
using Newtonsoft.Json;

namespace Artemis.Settings
{
    public class EffectSettings : IArtemisSettings
    {
        [DefaultValue("Default")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string LastProfile { get; set; }

        public void Save()
        {
            SettingsProvider.Save(this);
        }

        public void Reset(bool save = false)
        {
            JsonConvert.PopulateObject("{}", this, new JsonSerializerSettings
            {
                ObjectCreationHandling = ObjectCreationHandling.Reuse
            });

            if (save)
                SettingsProvider.Save(this);
        }
    }
}