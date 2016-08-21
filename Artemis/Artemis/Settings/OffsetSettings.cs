using Artemis.DAL;
using Artemis.Utilities.Memory;
using Newtonsoft.Json;

namespace Artemis.Settings
{
    public class OffsetSettings : IArtemisSettings
    {
        public GamePointersCollection RocketLeague { get; set; }

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