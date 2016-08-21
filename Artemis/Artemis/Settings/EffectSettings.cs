using Artemis.DAL;
using Newtonsoft.Json;

namespace Artemis.Settings
{
    public class EffectSettings : IArtemisSettings
    {
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