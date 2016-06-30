using System.Windows;
using System.Windows.Media;
using Artemis.Layers.Interfaces;
using Artemis.Models.Interfaces;
using Artemis.Models.Profiles;
using Artemis.Properties;
using Artemis.Utilities;

namespace Artemis.Layers.Types
{
    public class FolderType : ILayerType
    {
        public string Name { get; } = "Folder";
        public bool MustDraw { get; } = false;

        public ImageSource DrawThumbnail(LayerModel layer)
        {
            var thumbnailRect = new Rect(0, 0, 18, 18);
            var visual = new DrawingVisual();
            using (var c = visual.RenderOpen())
                c.DrawImage(ImageUtilities.BitmapToBitmapImage(Resources.folder), thumbnailRect);

            var image = new DrawingImage(visual.Drawing);
            return image;
        }

        public void Draw(LayerModel layer, DrawingContext c)
        {
            // Draw children?
        }

        public void Update(LayerModel layerModel, IDataModel dataModel, bool isPreview = false)
        {
            // Update children?
        }

        public void SetupProperties(LayerModel layerModel)
        {
        }
    }
}