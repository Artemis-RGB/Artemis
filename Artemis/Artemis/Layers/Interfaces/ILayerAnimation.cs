using System.Windows.Media;
using Artemis.Models.Profiles;
using Artemis.Models.Profiles.Layers;

namespace Artemis.Layers.Interfaces
{
    public interface ILayerAnimation
    {
        string Name { get; }
        void Update(LayerModel layerModel, bool updateAnimations);
        void Draw(LayerPropertiesModel props, LayerPropertiesModel applied, DrawingContext c);
    }
}