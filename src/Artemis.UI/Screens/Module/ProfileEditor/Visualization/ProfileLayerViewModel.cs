using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.LayerShapes;
using Artemis.Core.Models.Surface;
using Artemis.UI.Extensions;
using RGB.NET.Core;
using Rectangle = Artemis.Core.Models.Profile.LayerShapes.Rectangle;

namespace Artemis.UI.Screens.Module.ProfileEditor.Visualization
{
    public class ProfileLayerViewModel : CanvasViewModel
    {
        public ProfileLayerViewModel(Layer layer)
        {
            Layer = layer;

            Update();
            Layer.RenderPropertiesUpdated += LayerOnRenderPropertiesUpdated;
        }

        public Layer Layer { get; }

        public Geometry LayerGeometry { get; set; }
        public Geometry OpacityGeometry { get; set; }
        public Geometry ShapeGeometry { get; set; }
        public Rect ViewportRectangle { get; set; }

        private void Update()
        {
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

            var layerGeometry = Geometry.Empty;
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

                layerGeometry = Geometry.Combine(layerGeometry, geometry, GeometryCombineMode.Union, null, 5, ToleranceType.Absolute);
            }

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

            var x = Layer.Leds.Min(l => l.RgbLed.AbsoluteLedRectangle.Location.X);
            var y = Layer.Leds.Min(l => l.RgbLed.AbsoluteLedRectangle.Location.Y);
            var width = Layer.Leds.Max(l => l.RgbLed.AbsoluteLedRectangle.Location.X + l.RgbLed.AbsoluteLedRectangle.Size.Width) - x;
            var height = Layer.Leds.Max(l => l.RgbLed.AbsoluteLedRectangle.Location.Y + l.RgbLed.AbsoluteLedRectangle.Size.Height) - y;

            var rect = new Rect(
                x + width * Layer.LayerShape.Position.X,
                y + height * Layer.LayerShape.Position.Y,
                width * Layer.LayerShape.Size.Width,
                height * Layer.LayerShape.Size.Height
            );
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
            if (!Layer.Leds.Any())
            {
                ViewportRectangle = Rect.Empty;
                return;
            }

            var x = Layer.Leds.Min(l => l.RgbLed.AbsoluteLedRectangle.Location.X);
            var y = Layer.Leds.Min(l => l.RgbLed.AbsoluteLedRectangle.Location.Y);
            var width = Layer.Leds.Max(l => l.RgbLed.AbsoluteLedRectangle.Location.X + l.RgbLed.AbsoluteLedRectangle.Size.Width) - x;
            var height = Layer.Leds.Max(l => l.RgbLed.AbsoluteLedRectangle.Location.Y + l.RgbLed.AbsoluteLedRectangle.Size.Height) - y;
            ViewportRectangle = new Rect(x - x * Layer.LayerShape.Position.X, y - y * Layer.LayerShape.Position.Y, width, height);
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

        private void LayerOnRenderPropertiesUpdated(object sender, EventArgs e)
        {
            Update();
        }

        public void Dispose()
        {
            Layer.RenderPropertiesUpdated -= LayerOnRenderPropertiesUpdated;
        }
    }
}