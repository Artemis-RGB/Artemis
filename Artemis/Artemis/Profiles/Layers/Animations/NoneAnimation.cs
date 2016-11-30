using System.Windows.Media;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Models;

namespace Artemis.Profiles.Layers.Animations
{
    public class NoneAnimation : ILayerAnimation
    {
        public string Name => "None";

        public void Update(LayerModel layerModel, bool updateAnimations)
        {
        }

        public void Draw(LayerModel layerModel, DrawingContext c)
        {
        }

        public bool MustExpire(LayerModel layer)
        {
            return true;
        }
    }
}