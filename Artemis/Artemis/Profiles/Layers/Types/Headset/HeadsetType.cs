using System;
using System.Collections.Generic;
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
            // If an animation is present, let it handle the drawing
            if (layer.LayerAnimation != null)
            {
                layer.LayerAnimation.Draw(layer.Properties, layer.AppliedProperties, c);
                return;
            }

            // Otherwise draw the rectangle with its applied dimensions and brush
            var rect = new Rect(layer.AppliedProperties.X * 4,
                layer.AppliedProperties.Y * 4,
                layer.AppliedProperties.Width * 4,
                layer.AppliedProperties.Height * 4);

            c.PushClip(new RectangleGeometry(rect));
            c.DrawRectangle(layer.AppliedProperties.Brush, null, rect);
            c.Pop();
        }

        public void Update(LayerModel layerModel, IDataModel dataModel, bool isPreview = false)
        {
            layerModel.AppliedProperties = GeneralHelpers.Clone(layerModel.Properties);

            // Headset layers are always drawn 10*10 (which is 40*40 when scaled up)
            layerModel.AppliedProperties.Width = 10;
            layerModel.AppliedProperties.Height = 10;
            layerModel.AppliedProperties.X = 0;
            layerModel.AppliedProperties.Y = 0;

            if (isPreview || dataModel == null)
                return;

            // If not previewing, apply dynamic properties according to datamodel
            var props = (SimplePropertiesModel)layerModel.AppliedProperties;
            foreach (var dynamicProperty in props.DynamicProperties)
                dynamicProperty.ApplyProperty(dataModel, layerModel.AppliedProperties);
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
            if (layerPropertiesViewModel is HeadsetPropertiesViewModel)
                return layerPropertiesViewModel;
            return new HeadsetPropertiesViewModel(proposedLayer, dataModel, layerAnimations);
        }
    }
}