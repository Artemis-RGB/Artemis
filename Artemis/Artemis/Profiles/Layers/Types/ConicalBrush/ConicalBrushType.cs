using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Artemis.Modules.Abstract;
using Artemis.Profiles.Layers.Abstract;
using Artemis.Profiles.Layers.Animations;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Models;
using Artemis.Profiles.Layers.Types.ConicalBrush.Drawing;
using Artemis.ViewModels;

namespace Artemis.Profiles.Layers.Types.ConicalBrush
{
    public class ConicalBrushType : ILayerType
    {
        #region Properties & Fields

        private ConicalGradientDrawer _conicalGradientDrawer;
        private ConicalGradientDrawer _conicalGradientDrawerThumbnail;

        public string Name => "Conical Brush";
        public bool ShowInEdtor => true;
        public DrawType DrawType => DrawType.Keyboard;

        #endregion

        public ConicalBrushType()
        {
            _conicalGradientDrawer = new ConicalGradientDrawer();
            _conicalGradientDrawerThumbnail = new ConicalGradientDrawer(18, 18);
        }

        #region Methods

        public ImageSource DrawThumbnail(LayerModel layer)
        {
            _conicalGradientDrawerThumbnail.GradientStops = GetGradientStops(layer.Brush).Select(x => new Tuple<double, Color>(x.Offset, x.Color)).ToList();
            _conicalGradientDrawerThumbnail.Update();

            Rect thumbnailRect = new Rect(0, 0, 18, 18);
            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext c = visual.RenderOpen())
                if (_conicalGradientDrawerThumbnail.Brush != null)
                    c.DrawRectangle(_conicalGradientDrawerThumbnail.Brush.Clone(), new Pen(new SolidColorBrush(Colors.White), 1), thumbnailRect);

            DrawingImage image = new DrawingImage(visual.Drawing);
            return image;
        }

        public void Draw(LayerModel layerModel, DrawingContext c)
        {
            ConicalBrushPropertiesModel properties = layerModel.Properties as ConicalBrushPropertiesModel;
            if (properties == null) return;

            Brush origBrush = layerModel.Brush;

            _conicalGradientDrawer.GradientStops = GetGradientStops(layerModel.Brush).Select(x => new Tuple<double, Color>(x.Offset, x.Color)).ToList();
            _conicalGradientDrawer.Update();
            layerModel.Brush = _conicalGradientDrawer.Brush;

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
            if (layerModel.Properties is ConicalBrushPropertiesModel)
                return;

            layerModel.Properties = new ConicalBrushPropertiesModel(layerModel.Properties);
        }

        public LayerPropertiesViewModel SetupViewModel(LayerEditorViewModel layerEditorViewModel, LayerPropertiesViewModel layerPropertiesViewModel)
        {
            return (layerPropertiesViewModel as ConicalBrushPropertiesViewModel) ?? new ConicalBrushPropertiesViewModel(layerEditorViewModel);
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
