using System.Windows;
using System.Windows.Media;
using Artemis.Modules.Abstract;
using Artemis.Profiles.Layers.Abstract;
using Artemis.Profiles.Layers.Animations;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Models;
using Artemis.Profiles.Layers.Types.AngularBrush.Drawing;
using Artemis.ViewModels;

namespace Artemis.Profiles.Layers.Types.AngularBrush
{
    public class AngularBrushType : ILayerType
    {
        #region Properties & Fields

        private GradientDrawer _gradientDrawer;

        public string Name => "Angular Brush";
        public bool ShowInEdtor => true;
        public DrawType DrawType => DrawType.Keyboard;

        #endregion

        public AngularBrushType()
        {
            _gradientDrawer = new GradientDrawer();
        }

        #region Methods

        public ImageSource DrawThumbnail(LayerModel layer)
        {
            //TODO DarthAffe 14.01.2017: This could be replaced with the real brush but it complaints about the thread too
            Rect thumbnailRect = new Rect(0, 0, 18, 18);
            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext c = visual.RenderOpen())
                if (layer.Properties.Brush != null)
                    c.DrawRectangle(layer.Properties.Brush,
                        new Pen(new SolidColorBrush(Colors.White), 1),
                        thumbnailRect);

            DrawingImage image = new DrawingImage(visual.Drawing);
            return image;
        }

        public void Draw(LayerModel layerModel, DrawingContext c)
        {
            AngularBrushPropertiesModel properties = layerModel.Properties as AngularBrushPropertiesModel;
            if (properties == null) return;

            Brush origBrush = layerModel.Brush;

            _gradientDrawer.GradientStops = properties.GradientStops;
            _gradientDrawer.Update();
            layerModel.Brush = _gradientDrawer.Brush.Clone();

            // If an animation is present, let it handle the drawing
            if (layerModel.LayerAnimation != null && !(layerModel.LayerAnimation is NoneAnimation))
            {
                layerModel.LayerAnimation.Draw(layerModel, c);
                return;
            }

            // Otherwise draw the rectangle with its layer.AppliedProperties dimensions and brush
            Rect rect = layerModel.Properties.Contain
                ? layerModel.LayerRect()
                : new Rect(layerModel.Properties.X * 4, layerModel.Properties.Y * 4,
                    layerModel.Properties.Width * 4, layerModel.Properties.Height * 4);

            Rect clip = layerModel.LayerRect();

            // Can't meddle with the original brush because it's frozen.
            Brush brush = layerModel.Brush.Clone();
            brush.Opacity = layerModel.Opacity;

            c.PushClip(new RectangleGeometry(clip));
            c.DrawRectangle(brush, null, rect);
            c.Pop();

            layerModel.Brush = origBrush;
        }

        public void Update(LayerModel layerModel, ModuleDataModel dataModel, bool isPreview = false)
        {
            layerModel.ApplyProperties(true);
            if (isPreview || dataModel == null)
                return;

            // If not previewing, apply dynamic properties according to datamodel
            foreach (DynamicPropertiesModel dynamicProperty in layerModel.Properties.DynamicProperties)
                dynamicProperty.ApplyProperty(dataModel, layerModel);
        }

        public void SetupProperties(LayerModel layerModel)
        {
            if (layerModel.Properties is AngularBrushPropertiesModel)
                return;

            layerModel.Properties = new AngularBrushPropertiesModel(layerModel.Properties);
        }

        public LayerPropertiesViewModel SetupViewModel(LayerEditorViewModel layerEditorViewModel, LayerPropertiesViewModel layerPropertiesViewModel)
        {
            return (layerPropertiesViewModel as AngularBrushPropertiesViewModel) ?? new AngularBrushPropertiesViewModel(layerEditorViewModel);
        }

        #endregion
    }
}
