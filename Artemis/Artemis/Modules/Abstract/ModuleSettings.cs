using System.ComponentModel;
using Artemis.DAL;
using Artemis.Settings;
using Newtonsoft.Json;

namespace Artemis.Modules.Abstract
{
    public abstract class ModuleSettings : IArtemisSettings
    {
        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool IsEnabled { get; set; }

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