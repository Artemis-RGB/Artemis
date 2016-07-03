using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Artemis.Models.Interfaces;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Models;
using Artemis.Properties;
using Artemis.Utilities;
using Artemis.ViewModels.Profiles.Layers;

namespace Artemis.Profiles.Layers.Types.Folder
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
        }

        public void Update(LayerModel layerModel, IDataModel dataModel, bool isPreview = false)
        {
        }

        public void SetupProperties(LayerModel layerModel)
        {
            if (layerModel.Properties is SimplePropertiesModel)
                return;

            layerModel.Properties = new SimplePropertiesModel(layerModel.Properties);
        }

        public LayerPropertiesViewModel SetupViewModel(LayerPropertiesViewModel layerPropertiesViewModel,
            List<ILayerAnimation> layerAnimations, IDataModel dataModel, LayerModel proposedLayer)
        {
            if (layerPropertiesViewModel is FolderPropertiesViewModel)
                return layerPropertiesViewModel;
            return new FolderPropertiesViewModel(proposedLayer, dataModel);
        }
    }
}