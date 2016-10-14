using System.Windows.Media;
using Artemis.DAL;
using Artemis.Settings;
using Newtonsoft.Json;

namespace Artemis.Modules.Overlays.VolumeDisplay
{
    public class VolumeDisplaySettings : OverlaySettings
    {
        public Color MainColor { get; set; }
        public Color SecondaryColor { get; set; }

        public new void Reset(bool save = false)
        {
            JsonConvert.PopulateObject("{}", this, new JsonSerializerSettings
            {
                ObjectCreationHandling = ObjectCreationHandling.Reuse
            });

            MainColor = Colors.Red;
            SecondaryColor = Colors.GreenYellow;

            if (save)
                SettingsProvider.Save(this);
        }
    }
}