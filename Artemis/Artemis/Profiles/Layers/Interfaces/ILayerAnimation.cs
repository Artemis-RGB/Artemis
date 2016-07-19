using System.Windows.Media;
using Artemis.Profiles.Layers.Models;
using Newtonsoft.Json;

namespace Artemis.Profiles.Layers.Interfaces
{
    public interface ILayerAnimation
    {
        [JsonIgnore]
        string Name { get; }
        void Update(LayerModel layerModel, bool updateAnimations);
        void Draw(LayerPropertiesModel props, LayerPropertiesModel applied, DrawingContext c);
        bool MustExpire(LayerModel layer);
    }
}