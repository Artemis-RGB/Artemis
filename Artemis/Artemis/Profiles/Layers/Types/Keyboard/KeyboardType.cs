using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Artemis.Models.Interfaces;
using Artemis.Profiles.Layers.Abstract;
using Artemis.Profiles.Layers.Animations;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Models;
using Artemis.ViewModels.Profiles;

namespace Artemis.Profiles.Layers.Types.Keyboard
{
    public class KeyboardType : ILayerType
    {
        public string Name { get; } = "Keyboard";
        public bool ShowInEdtor { get; } = true;
        public DrawType DrawType { get; } = DrawType.Keyboard;

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
            if (layer.LayerAnimation != null && !(layer.LayerAnimation is NoneAnimation))
            {
                layer.LayerAnimation.Draw(layer.Properties, layer.AppliedProperties, c);
                return;
            }

            // Otherwise draw the rectangle with its layer.AppliedProperties dimensions and brush
            var rect = layer.Properties.Contain
                ? new Rect(layer.AppliedProperties.X*4,
                    layer.AppliedProperties.Y*4,
                    layer.AppliedProperties.Width*4,
                    layer.AppliedProperties.Height*4)
                : new Rect(layer.Properties.X*4,
                    layer.Properties.Y*4,
                    layer.Properties.Width*4,
                    layer.Properties.Height*4);

            var clip = new Rect(layer.AppliedProperties.X*4, layer.AppliedProperties.Y*4,
                layer.AppliedProperties.Width*4, layer.AppliedProperties.Height*4);


            c.PushClip(new RectangleGeometry(clip));
            c.DrawRectangle(layer.AppliedProperties.Brush, null, rect);
            c.Pop();
        }

        public void Update(LayerModel layerModel, IDataModel dataModel, bool isPreview = false)
        {
            layerModel.AppliedProperties = new KeyboardPropertiesModel(layerModel.Properties);
            if (isPreview || dataModel == null)
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

            layerModel.Properties = new KeyboardPropertiesModel(layerModel.Properties);
        }

        public LayerPropertiesViewModel SetupViewModel(LayerEditorViewModel layerEditorViewModel,
            LayerPropertiesViewModel layerPropertiesViewModel)
        {
            var model = layerPropertiesViewModel as KeyboardPropertiesViewModel;
            if (model == null)
                return new KeyboardPropertiesViewModel(layerEditorViewModel)
                {
                    IsGif = false
                };

            model.IsGif = false;
            return layerPropertiesViewModel;
        }
    }
}