using System.Collections.Generic;
using System.Windows;
using Artemis.Profiles.Layers.Models;

namespace Artemis.Profiles.Layers.Types.Keyboard
{
    public class KeyboardPropertiesModel : LayerPropertiesModel
    {
        public KeyboardPropertiesModel()
        {
            DynamicProperties = new List<DynamicPropertiesModel>();
        }

        public string GifFile { get; set; }
        public List<DynamicPropertiesModel> DynamicProperties { get; set; }

        public Rect GetRect(int scale = 4)
        {
            return new Rect(X*scale, Y*scale, Width*scale, Height*scale);
        }
    }
}