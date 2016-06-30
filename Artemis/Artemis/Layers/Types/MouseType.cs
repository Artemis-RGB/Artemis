using System;
using System.Windows;
using System.Windows.Media;
using Artemis.Layers.Interfaces;
using Artemis.Models.Interfaces;
using Artemis.Models.Profiles;
using Artemis.Models.Profiles.Layers;
using Artemis.Properties;
using Artemis.Utilities;

namespace Artemis.Layers.Types
{
    public class MouseType : ILayerType
    {
        public string Name { get; } = "Mouse";
        public bool MustDraw { get; } = false;

        public ImageSource DrawThumbnail(LayerModel layer)
        {
            var thumbnailRect = new Rect(0, 0, 18, 18);
            var visual = new DrawingVisual();
            using (var c = visual.RenderOpen())
                c.DrawImage(ImageUtilities.BitmapToBitmapImage(Resources.mouse), thumbnailRect);

            var image = new DrawingImage(visual.Drawing);
            return image;
        }

        public void Draw(LayerModel layer, DrawingContext c)
        {
            throw new NotImplementedException();
        }

        public void Update(LayerModel layerModel, IDataModel dataModel, bool isPreview = false)
        {
            throw new NotImplementedException();
        }

        public void SetupProperties(LayerModel layerModel)
        {
            if (layerModel.Properties is SimplePropertiesModel)
                return;

            var brush = new SolidColorBrush(ColorHelpers.GetRandomRainbowMediaColor());
            layerModel.Properties = new SimplePropertiesModel {Brush = brush, Opacity = 1};
        }
    }
}