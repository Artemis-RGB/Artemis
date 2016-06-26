using System.Windows.Media;
using Artemis.Models.Interfaces;
using Artemis.Models.Profiles;

namespace Artemis.Layers.Interfaces
{
    public interface ILayerType
    {
        string Name { get; }
        bool MustDraw(IDataModel dataModel, LayerModel layer);
        void Draw(DrawingContext c, LayerModel layer);
    }
}