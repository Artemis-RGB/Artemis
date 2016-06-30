using System.Windows;
using System.Windows.Media;
using Artemis.Layers.Interfaces;
using Artemis.Models.Interfaces;
using Artemis.Models.Profiles;
using Artemis.Models.Profiles.Layers;
using Artemis.Utilities;

namespace Artemis.Layers.Types
{
    public class KeyboardType : ILayerType
    {
        public string Name { get; } = "Keyboard";
        public bool MustDraw { get; } = true;

        public ImageSource DrawThumbnail(LayerModel layer)
        {
            var thumbnailRect = new Rect(0, 0, 18, 18);
            var visual = new DrawingVisual();
            using (var c = visual.RenderOpen())
            {
                if (layer.Properties.Brush != null)
                {
                    c.DrawRectangle(layer.Properties.Brush,
                        new Pen(new SolidColorBrush(Colors.White), 1),
                        thumbnailRect);
                }
            }

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
            layerModel.AppliedProperties = GeneralHelpers.Clone(layerModel.Properties);
            if (isPreview)
                return;

            // If not previewing, apply dynamic properties according to datamodel
            var keyboardProps = (KeyboardPropertiesModel) layerModel.AppliedProperties;
            foreach (var dynamicProperty in keyboardProps.DynamicProperties)
                dynamicProperty.ApplyProperty(dataModel, layerModel.AppliedProperties);
        }

        public void SetupProperties(LayerModel layerModel)
        {
            if (layerModel.Properties is KeyboardPropertiesModel)
                return;

            var brush = new SolidColorBrush(ColorHelpers.GetRandomRainbowMediaColor());
            layerModel.Properties = new KeyboardPropertiesModel
            {
                Brush = brush,
                Height = 1,
                Width = 1,
                X = 0,
                Y = 0,
                Opacity = 1
            };
        }
    }
}