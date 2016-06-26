using System.Windows.Media;
using Artemis.Models.Profiles.Layers;

namespace Artemis.Layers.Interfaces
{
    public interface ILayerAnimation
    {
        string Name { get; }
        void Update(LayerPropertiesModel properties, bool updateAnimations);
        void Draw(DrawingContext c, KeyboardPropertiesModel props, AppliedProperties applied);
    }
}