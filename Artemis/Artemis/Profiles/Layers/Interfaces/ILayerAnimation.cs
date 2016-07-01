using System.Windows.Media;
using Artemis.Profiles.Layers.Models;

namespace Artemis.Profiles.Layers.Interfaces
{
    public interface ILayerAnimation
    {
        string Name { get; }
        void Update(LayerModel layerModel, bool updateAnimations);
        void Draw(LayerPropertiesModel props, LayerPropertiesModel applied, DrawingContext c);
        bool MustExpire(LayerModel layer);
    }
}