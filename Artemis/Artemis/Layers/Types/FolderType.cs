using System;
using System.Windows.Media;
using Artemis.Layers.Interfaces;
using Artemis.Models.Interfaces;
using Artemis.Models.Profiles;

namespace Artemis.Layers.Types
{
    public class FolderType : ILayerType
    {
        public string Name { get; } = "Folder";

        public bool MustDraw(IDataModel dataModel, LayerModel layer)
        {
            throw new NotImplementedException();
        }

        public void Draw(DrawingContext c, LayerModel layer)
        {
            throw new NotImplementedException();
        }
    }
}