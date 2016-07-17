using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Artemis.Models.Interfaces;
using Artemis.Profiles.Layers.Abstract;
using Artemis.Profiles.Layers.Animations;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Models;
using Artemis.Properties;
using Artemis.Utilities;

namespace Artemis.Profiles.Layers.Types.Headset
{
    public class HeadsetType : ILayerType
    {
        public string Name { get; } = "Headset";
        public bool ShowInEdtor { get; } = false;
        public DrawType DrawType { get; } = DrawType.Headset;

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
            if (layer.LayerAnimation != null && !(layer.LayerAnimation is NoneAnimation))
            {
                layer.LayerAnimation.Draw(layer.Properties, layer.AppliedProperties, c);
                return;
            }

            // Otherwise draw the rectangle with its applied dimensions and brush
            var rect = new Rect(layer.AppliedProperties.X*4,
                layer.AppliedProperties.Y*4,
                layer.AppliedProperties.Width*4,
                layer.AppliedProperties.Height*4);

            c.PushClip(new RectangleGeometry(rect));
            c.DrawRectangle(layer.AppliedProperties.Brush, null, rect);
            c.Pop();
        }

        public void Update(LayerModel layerModel, IDataModel dataModel, bool isPreview = false)
        {
            // Headset layers are always drawn 10*10 (which is 40*40 when scaled up)
            layerModel.Properties.Width = 10;
            layerModel.Properties.Height = 10;
            layerModel.Properties.X = 0;
            layerModel.Properties.Y = 0;
            layerModel.Properties.Contain = true;

            layerModel.AppliedProperties = new SimplePropertiesModel(layerModel.Properties);

            if (isPreview || dataModel == null)
                return;

            // If not previewing, apply dynamic properties according to datamodel
            var props = (SimplePropertiesModel) layerModel.AppliedProperties;
            foreach (var dynamicProperty in props.DynamicProperties)
                dynamicProperty.ApplyProperty(dataModel, layerModel.AppliedProperties);
        }

        public void SetupProperties(LayerModel layerModel)
        {
            if (layerModel.Properties is SimplePropertiesModel)
                return;

            layerModel.Properties = new SimplePropertiesModel(layerModel.Properties);

            // Remove height and width dynamic properties since they are not applicable
            layerModel.Properties.DynamicProperties.Remove(
                layerModel.Properties.DynamicProperties.FirstOrDefault(d => d.LayerProperty == "Height"));
            layerModel.Properties.DynamicProperties.Remove(
                layerModel.Properties.DynamicProperties.FirstOrDefault(d => d.LayerProperty == "Width"));
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