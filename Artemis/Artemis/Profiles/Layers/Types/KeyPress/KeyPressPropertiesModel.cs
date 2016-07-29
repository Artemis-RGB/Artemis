using Artemis.Profiles.Layers.Models;

namespace Artemis.Profiles.Layers.Types.KeyPress
{
    public class KeyPressPropertiesModel : LayerPropertiesModel
    {
        public KeyPressPropertiesModel(LayerPropertiesModel properties) : base(properties)
        {
        }

        public int Scale { get; set; }
        public bool RandomColor { get; set; }
    }
}