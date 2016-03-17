using System.Collections.Generic;
using System.Drawing;
using Artemis.Components.Abstract;

namespace Artemis.Components
{
    public class LayerComposite : LayerComponent
    {
        public List<LayerComponent> LayerComponents { get; set; }

        public override void Draw(Graphics g)
        {
            foreach (var layerComponent in LayerComponents)
                layerComponent.Draw(g);
        }
    }
}