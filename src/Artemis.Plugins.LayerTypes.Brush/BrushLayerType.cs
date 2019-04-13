using System.Drawing;
using Artemis.Core.Plugins.Interfaces;
using Artemis.Core.ProfileElements;
using QRCoder;
using RGB.NET.Core;

namespace Artemis.Plugins.LayerTypes.Brush
{
    public class BrushLayerType : ILayerType
    {
        public void Dispose()
        {
        }

        public void EnablePlugin()
        {
            var qrGenerator = new QRCodeGenerator();
        }

        public void Update(Layer layer)
        {
            var config = layer.LayerTypeConfiguration as BrushConfiguration;
            if (config == null)
                return;

            // Update the brush
        }

        public void Render(Layer device, RGBSurface surface, Graphics graphics)
        {
        }
    }
}