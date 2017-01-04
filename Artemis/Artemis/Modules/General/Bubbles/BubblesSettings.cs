using System.ComponentModel;
using System.Windows.Media;
using Artemis.DAL;
using Artemis.Modules.Abstract;
using Newtonsoft.Json;

namespace Artemis.Modules.General.Bubbles
{
    public class BubblesSettings : ModuleSettings
    {
        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool IsRandomColors { get; set; }

        public Color BubbleColor { get; set; }

        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool IsShiftColors { get; set; }

        [DefaultValue(12)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int ShiftColorSpeed { get; set; }

        [DefaultValue(25)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int BubbleSize { get; set; }

        [DefaultValue(4)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int MoveSpeed { get; set; }

        [DefaultValue(14)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int BubbleCount { get; set; }

        [DefaultValue(25)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int Smoothness { get; set; }

        public new void Reset(bool save = false)
        {
            JsonConvert.PopulateObject("{}", this, new JsonSerializerSettings
            {
                ObjectCreationHandling = ObjectCreationHandling.Reuse
            });

            BubbleColor = Colors.Red;

            if (save)
                SettingsProvider.Save(this);
        }
    }
}