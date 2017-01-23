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
        void Draw(LayerModel layerModel, DrawingContext c, int drawScale);
        bool MustExpire(LayerModel layerModel);
    }
}