using System.Collections.Generic;
using Artemis.Core.Plugins.LayerElement;
using Artemis.Core.Utilities;
using SkiaSharp;

namespace Artemis.Plugins.LayerElements.Noise
{
    public class NoiseLayerElementViewModel : LayerElementViewModel
    {
        public NoiseLayerElementViewModel(NoiseLayerElement layerElement) : base(layerElement)
        {
            LayerElement = layerElement;
        }

        public new NoiseLayerElement LayerElement { get; }
        public IEnumerable<ValueDescription> BlendModes => EnumUtilities.GetAllValuesAndDescriptions(typeof(SKBlendMode));
    }
}