using Artemis.Profiles.Layers.Models;

namespace Artemis.Profiles.Layers.Types.Keyboard
{
    public class KeyboardPropertiesModel : LayerPropertiesModel
    {
        public KeyboardPropertiesModel(LayerPropertiesModel properties = null) : base(properties)
        {
        }

        public string GifFile { get; set; }
    }
}