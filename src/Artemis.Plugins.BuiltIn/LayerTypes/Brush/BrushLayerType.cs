using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Artemis.Core.Plugins.Interfaces;
using Artemis.Core.ProfileElements;
using RGB.NET.Core;

namespace Artemis.Plugins.BuiltIn.LayerTypes.Brush
{
    public class BrushLayerType : ILayerType
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void LoadPlugin()
        {
            throw new NotImplementedException();
        }

        public void Update(Layer layer)
        {
            var config = layer.LayerTypeConfiguration as BrushConfiguration;
            if (config == null)
                return;

            // Update the brush
        }

        public void Render(Layer device, RGBSurface surface)
        {
        }
    }
}
