using System;
using System.Windows.Media;
using Artemis.Layers.Interfaces;
using Artemis.Models.Interfaces;
using Artemis.Models.Profiles;

namespace Artemis.Layers.Types
{
    public class KeyboardType : ILayerType
    {
        public string Name { get; } = "Keyboard";

        public bool MustDraw(IDataModel dataModel, LayerModel layer)
        {
            throw new NotImplementedException();
        }

        public void Draw(DrawingContext c, LayerModel layer)
        {
            // Check if must be drawn
            throw new NotImplementedException();
        }
    }
}