using System.Linq;
using System.Windows;
using System.Windows.Media;
using Artemis.Modules.Abstract;
using Artemis.Profiles.Layers.Abstract;
using Artemis.Profiles.Layers.Animations;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Models;
using Artemis.Properties;
using Artemis.Utilities;
using Artemis.ViewModels;

namespace Artemis.Profiles.Layers.Types.Mousemat
{
    public class MousematType : ILayerType
    {
        public string Name => "Mousemat";
        public bool ShowInEdtor => false;
        public DrawType DrawType => DrawType.Mousemat;
        public int DrawScale => 2;

        public ImageSource DrawThumbnail(LayerModel layer)
        {
            var thumbnailRect = new Rect(0, 0, 18, 18);
            var visual = new DrawingVisual();
            using (var c = visual.RenderOpen())
                c.DrawImage(ImageUtilities.BitmapToBitmapImage(Resources.mousemat), thumbnailRect);

            var image = new DrawingImage(visual.Drawing);
            return image;
        }

        public void Draw(LayerModel layerModel, DrawingContext c)
        {
            // If an animation is present, let it handle the drawing
            if (layerModel.LayerAnimation != null && !(layerModel.LayerAnimation is NoneAnimation))
            {
                layerModel.LayerAnimation.Draw(layerModel, c, DrawScale);
                return;
            }

            // Otherwise draw the rectangle with its applied dimensions and brush
            var rect = layerModel.LayerRect(DrawScale);

            // Can't meddle with the original brush because it's frozen.
            var brush = layerModel.Brush.Clone();
            brush.Opacity = layerModel.Opacity;

            c.PushClip(new RectangleGeometry(rect));
            c.DrawRectangle(brush, null, rect);
            c.Pop();
        }

        public void Update(LayerModel layerModel, ModuleDataModel dataModel, bool isPreview = false)
        {
            // Mousemat layers are always drawn 10*10 (which is 40*40 when scaled up)
            layerModel.Properties.Width = 10;
            layerModel.Properties.Height = 10;
            layerModel.Properties.X = 0;
            layerModel.Properties.Y = 0;
            layerModel.Properties.Contain = true;

            layerModel.ApplyProperties(true);

            if (isPreview || dataModel == null)
                return;

            // If not previewing, apply dynamic properties according to datamodel
            foreach (var dynamicProperty in layerModel.Properties.DynamicProperties)
                dynamicProperty.ApplyProperty(dataModel, layerModel);
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

        public LayerPropertiesViewModel SetupViewModel(LayerEditorViewModel layerEditorViewModel,
            LayerPropertiesViewModel layerPropertiesViewModel)
        {
            if (layerPropertiesViewModel is MousematPropertiesViewModel)
                return layerPropertiesViewModel;
            return new MousematPropertiesViewModel(layerEditorViewModel);
        }
    }
}