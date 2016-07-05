using System.Windows.Media;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Models;

namespace Artemis.Profiles.Layers.Animations
{
    public class NoneAnimation : ILayerAnimation
    {
        public string Name { get; } = "None";

        public void Update(LayerModel layerModel, bool updateAnimations)
        {
        }

        public void Draw(LayerPropertiesModel props, LayerPropertiesModel applied, DrawingContext c)
        {
        }

        public bool MustExpire(LayerModel layer)
        {
            return true;
        }
    }
}