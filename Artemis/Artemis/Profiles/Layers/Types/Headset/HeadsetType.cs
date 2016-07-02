using System;
using System.Windows;
using System.Windows.Media;
using Artemis.Models.Interfaces;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Models;
using Artemis.Properties;
using Artemis.Utilities;
using Artemis.ViewModels.Profiles.Layers;

namespace Artemis.Profiles.Layers.Types.Headset
{
    public class HeadsetType : ILayerType
    {
        public string Name { get; } = "Headset";
        public bool MustDraw { get; } = false;

        public ImageSource DrawThumbnail(LayerModel layer)
        {
            var thumbnailRect = new Rect(0, 0, 18, 18);
            var visual = new DrawingVisual();
            using (var c = visual.RenderOpen())
                c.DrawImage(ImageUtilities.BitmapToBitmapImage(Resources.headset), thumbnailRect);

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

        public LayerPropertiesViewModel SetupViewModel(LayerPropertiesViewModel layerPropertiesViewModel,
            IDataModel dataModel, LayerModel proposedLayer)
        {
            if (layerPropertiesViewModel is HeadsetPropertiesViewModel)
                return layerPropertiesViewModel;
            return new HeadsetPropertiesViewModel(dataModel, proposedLayer.Properties);
        }
    }
}