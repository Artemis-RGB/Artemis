using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.LayerShapes;
using Artemis.Core.Models.Surface;
using Artemis.UI.Extensions;
using Artemis.UI.Services.Interfaces;
using RGB.NET.Core;
using Rectangle = Artemis.Core.Models.Profile.LayerShapes.Rectangle;

namespace Artemis.UI.Screens.Module.ProfileEditor.Visualization
{
    public class ProfileLayerViewModel : CanvasViewModel
    {
        private readonly IProfileEditorService _profileEditorService;

        public ProfileLayerViewModel(Layer layer, IProfileEditorService profileEditorService)
        {
            _profileEditorService = profileEditorService;
            Layer = layer;

            Update();
            Layer.RenderPropertiesUpdated += LayerOnRenderPropertiesUpdated;
            _profileEditorService.SelectedProfileElementChanged += OnSelectedProfileElementChanged;
            _profileEditorService.SelectedProfileElementUpdated += OnSelectedProfileElementUpdated;
            _profileEditorService.ProfilePreviewUpdated += ProfileEditorServiceOnProfilePreviewUpdated;
        }


        public Layer Layer { get; }

        public Geometry LayerGeometry { get; set; }
        public Geometry OpacityGeometry { get; set; }
        public Geometry ShapeGeometry { get; set; }
        public Rect ViewportRectangle { get; set; }
        public bool IsSelected { get; set; }

        private void Update()
        {
            IsSelected = _profileEditorService.SelectedProfileElement == Layer;
            CreateLayerGeometry();
            CreateShapeGeometry();
            CreateViewportRectangle();
        }

        private void CreateLayerGeometry()
        {
            if (!Layer.Leds.Any())
            {
                LayerGeometry = Geometry.Empty;
                OpacityGeometry = Geometry.Empty;
                ViewportRectangle = Rect.Empty;
                return;
            }

            var group = new GeometryGroup();
            group.FillRule = FillRule.Nonzero;

            foreach (var led in Layer.Leds)
            {
                Geometry geometry;
                switch (led.RgbLed.Shape)
                {
                    case Shape.Custom:
                        if (led.RgbLed.Device.DeviceInfo.DeviceType == RGBDeviceType.Keyboard || led.RgbLed.Device.DeviceInfo.DeviceType == RGBDeviceType.Keypad)
                            geometry = CreateCustomGeometry(led, 2);
                        else
                            geometry = CreateCustomGeometry(led, 1);
                        break;
                    case Shape.Rectangle:
                        geometry = CreateRectangleGeometry(led);
                        break;
                    case Shape.Circle:
                        geometry = CreateCircleGeometry(led);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                group.Children.Add(geometry);
            }

            var layerGeometry = group.GetOutlinedPathGeometry();

            var opacityGeometry = Geometry.Combine(Geometry.Empty, layerGeometry, GeometryCombineMode.Exclude, new TranslateTransform());
            layerGeometry.Freeze();
            opacityGeometry.Freeze();
            LayerGeometry = layerGeometry;
            OpacityGeometry = opacityGeometry;
        }

        private void CreateShapeGeometry()
        {
            if (Layer.LayerShape == null || !Layer.Leds.Any())
            {
                ShapeGeometry = Geometry.Empty;
                return;
            }

            var skRect = Layer.LayerShape.GetUnscaledRectangle();
            var rect = new Rect(skRect.Left, skRect.Top, Math.Max(0, skRect.Width), Math.Max(0, skRect.Height));

            var shapeGeometry = Geometry.Empty;
            switch (Layer.LayerShape)
            {
                case Ellipse _:
                    shapeGeometry = new EllipseGeometry(rect);
                    break;
                case Fill _:
                    shapeGeometry = LayerGeometry;
                    break;
                case Polygon _:
                    // TODO
                    shapeGeometry = new RectangleGeometry(rect);
                    break;
                case Rectangle _:
                    shapeGeometry = new RectangleGeometry(rect);
                    break;
            }

            shapeGeometry.Freeze();
            ShapeGeometry = shapeGeometry;
        }

        private void CreateViewportRectangle()
        {
            if (!Layer.Leds.Any() || Layer.LayerShape == null)
            {
                ViewportRectangle = Rect.Empty;
                return;
            }

            var x = Layer.Leds.Min(l => l.RgbLed.AbsoluteLedRectangle.Location.X);
            var y = Layer.Leds.Min(l => l.RgbLed.AbsoluteLedRectangle.Location.Y);
            var width = Layer.Leds.Max(l => l.RgbLed.AbsoluteLedRectangle.Location.X + l.RgbLed.AbsoluteLedRectangle.Size.Width) - x;
            var height = Layer.Leds.Max(l => l.RgbLed.AbsoluteLedRectangle.Location.Y + l.RgbLed.AbsoluteLedRectangle.Size.Height) - y;

            var rect = new Rect(x, y, width, height);
            ViewportRectangle = rect;
        }

        private Geometry CreateRectangleGeometry(ArtemisLed led)
        {
            var rect = led.RgbLed.AbsoluteLedRectangle.ToWindowsRect(1);
            rect.Inflate(1, 1);
            return new RectangleGeometry(rect);
        }

        private Geometry CreateCircleGeometry(ArtemisLed led)
        {
            var rect = led.RgbLed.AbsoluteLedRectangle.ToWindowsRect(1);
            rect.Inflate(1, 1);
            return new EllipseGeometry(rect);
        }

        private Geometry CreateCustomGeometry(ArtemisLed led, double deflateAmount)
        {
            var rect = led.RgbLed.AbsoluteLedRectangle.ToWindowsRect(1);
            rect.Inflate(1, 1);
            try
            {
                var geometry = Geometry.Combine(
                    Geometry.Empty,
                    Geometry.Parse(led.RgbLed.ShapeData),
                    GeometryCombineMode.Union,
                    new TransformGroup
                    {
                        Children = new TransformCollection
                        {
                            new ScaleTransform(rect.Width, rect.Height),
                            new TranslateTransform(rect.X, rect.Y)
                        }
                    }
                );

                return geometry;
            }
            catch (Exception)
            {
                return CreateRectangleGeometry(led);
            }
        }

        public void Dispose()
        {
            Layer.RenderPropertiesUpdated -= LayerOnRenderPropertiesUpdated;
        }

        #region Event handlers  

        private void LayerOnRenderPropertiesUpdated(object sender, EventArgs e)
        {
            Update();
        }

        private void OnSelectedProfileElementChanged(object sender, EventArgs e)
        {
            IsSelected = _profileEditorService.SelectedProfileElement == Layer;
        }

        private void OnSelectedProfileElementUpdated(object sender, EventArgs e)
        {
            if (IsSelected)
                Update();
        }

        private void ProfileEditorServiceOnProfilePreviewUpdated(object sender, EventArgs e)
        {
            if (!IsSelected)
                return;
            CreateShapeGeometry();
            CreateViewportRectangle();
        }

        #endregion
    }
}