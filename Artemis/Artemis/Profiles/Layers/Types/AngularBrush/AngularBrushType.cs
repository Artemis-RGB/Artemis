using System;
using System.Linq;
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
        private GradientDrawer _gradientDrawerThumbnail;

        public string Name => "Angular Brush";
        public bool ShowInEdtor => true;
        public DrawType DrawType => DrawType.Keyboard;

        #endregion

        public AngularBrushType()
        {
            _gradientDrawer = new GradientDrawer();
            _gradientDrawerThumbnail = new GradientDrawer(18, 18);
        }

        #region Methods

        public ImageSource DrawThumbnail(LayerModel layer)
        {
            _gradientDrawerThumbnail.GradientStops = GetGradientStops(layer.Brush).Select(x => new Tuple<double, Color>(x.Offset, x.Color)).ToList();
            _gradientDrawerThumbnail.Update();

            Rect thumbnailRect = new Rect(0, 0, 18, 18);
            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext c = visual.RenderOpen())
                if (_gradientDrawerThumbnail.Brush != null)
                    c.DrawRectangle(_gradientDrawerThumbnail.Brush.Clone(), new Pen(new SolidColorBrush(Colors.White), 1), thumbnailRect);

            DrawingImage image = new DrawingImage(visual.Drawing);
            return image;
        }

        public void Draw(LayerModel layerModel, DrawingContext c)
        {
            AngularBrushPropertiesModel properties = layerModel.Properties as AngularBrushPropertiesModel;
            if (properties == null) return;

            Brush origBrush = layerModel.Brush;

            _gradientDrawer.GradientStops = GetGradientStops(layerModel.Brush).Select(x => new Tuple<double, Color>(x.Offset, x.Color)).ToList();
            _gradientDrawer.Update();
            layerModel.Brush = _gradientDrawer.Brush;

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

        private GradientStopCollection GetGradientStops(Brush brush)
        {
            LinearGradientBrush linearBrush = brush as LinearGradientBrush;
            if (linearBrush != null)
                return linearBrush.GradientStops;

            RadialGradientBrush radialBrush = brush as RadialGradientBrush;
            if (radialBrush != null)
                return radialBrush.GradientStops;

            SolidColorBrush solidBrush = brush as SolidColorBrush;
            if (solidBrush != null)
                return new GradientStopCollection(new[] { new GradientStop(solidBrush.Color, 0), new GradientStop(solidBrush.Color, 1) });

            return null;
        }

        #endregion
    }
}
