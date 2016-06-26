using System.Windows.Media;
using Artemis.Layers.Interfaces;
using Artemis.Models.Interfaces;
using Artemis.Models.Profiles;

namespace Artemis.Layers.Types
{
    public class HeadsetType : ILayerType
    {
        public string Name { get; } = "Headset";

        public bool MustDraw(IDataModel dataModel, LayerModel layer)
        {
            throw new System.NotImplementedException();
        }

        public void Draw(DrawingContext c, LayerModel layer)
        {
            throw new System.NotImplementedException();
        }
    }
}