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
        public string Name => "Keyboard";
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

        public void Draw(LayerModel layerModel, DrawingContext c)
        {
            // If an animation is present, let it handle the drawing
            if (layerModel.LayerAnimation != null && !(layerModel.LayerAnimation is NoneAnimation))
            {
                layerModel.LayerAnimation.Draw(layerModel, c);
                return;
            }

            // Otherwise draw the rectangle with its layer.AppliedProperties dimensions and brush
            var rect = layerModel.Properties.Contain
                ? layerModel.LayerRect()
                : new Rect(layerModel.Properties.X*4, layerModel.Properties.Y*4,
                    layerModel.Properties.Width*4, layerModel.Properties.Height*4);

            var clip = layerModel.LayerRect();

            // Can't meddle with the original brush because it's frozen.
            var brush = layerModel.Brush.Clone();
            brush.Opacity = layerModel.Opacity;

            c.PushClip(new RectangleGeometry(clip));
            c.DrawRectangle(brush, null, rect);
            c.Pop();
        }

        public void Update(LayerModel layerModel, IDataModel dataModel, bool isPreview = false)
        {
            layerModel.ApplyProperties(true);
            if (isPreview || dataModel == null)
                return;

            // If not previewing, apply dynamic properties according to datamodel
            foreach (var dynamicProperty in layerModel.Properties.DynamicProperties)
                dynamicProperty.ApplyProperty(dataModel, layerModel);
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