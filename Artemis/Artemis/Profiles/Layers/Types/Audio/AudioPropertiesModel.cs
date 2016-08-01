using Artemis.Profiles.Layers.Models;

namespace Artemis.Profiles.Layers.Types.Audio
{
    public class AudioPropertiesModel : LayerPropertiesModel
    {
        public AudioPropertiesModel(LayerPropertiesModel properties) : base(properties)
        {
        }

        public int Sensitivity { get; set; }
        public double FadeSpeed { get; set; }
    }
}