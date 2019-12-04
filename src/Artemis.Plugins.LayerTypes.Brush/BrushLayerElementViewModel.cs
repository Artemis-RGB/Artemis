using System.Collections.Generic;
using Artemis.Core.Plugins.LayerElement;
using Artemis.Core.Utilities;

namespace Artemis.Plugins.LayerElements.Brush
{
    public class BrushLayerElementViewModel : LayerElementViewModel
    {
        public BrushLayerElementViewModel(BrushLayerElement layerElement) : base(layerElement)
        {
            LayerElement = layerElement;
        }

        public new BrushLayerElement LayerElement { get; }
        public IEnumerable<ValueDescription> BrushTypes => EnumUtilities.GetAllValuesAndDescriptions(typeof(BrushType));
    }
}