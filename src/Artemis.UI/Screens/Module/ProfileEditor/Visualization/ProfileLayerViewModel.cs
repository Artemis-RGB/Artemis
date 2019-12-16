using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Surface;
using Artemis.UI.Extensions;
using RGB.NET.Core;

namespace Artemis.UI.Screens.Module.ProfileEditor.Visualization
{
    public class ProfileLayerViewModel : CanvasViewModel
    {
        public ProfileLayerViewModel(Layer layer)
        {
            Layer = layer;

            CreateLayerGeometry();
            Layer.RenderPropertiesUpdated += (sender, args) => CreateLayerGeometry();
        }

        public Layer Layer { get; }

        public Geometry LayerGeometry { get; set; }

        private void CreateLayerGeometry()
        {
            var layerGeometry = Geometry.Empty;
            
            foreach (var led in Layer.Leds)
            {
                Geometry geometry;
                switch (led.RgbLed.Shape)
                {
                    case Shape.Custom:
                        if (led.RgbLed.Device.DeviceInfo.DeviceType == RGBDeviceType.Keyboard || led.RgbLed.Device.DeviceInfo.DeviceType == RGBDeviceType.Keypad)
                            geometry = CreateCustomGeometry(led, 2.0);
                        else
                            geometry = CreateCustomGeometry(led, 1.0);
                        break;
                    case Shape.Rectangle:
                        if (led.RgbLed.Device.DeviceInfo.DeviceType == RGBDeviceType.Keyboard || led.RgbLed.Device.DeviceInfo.DeviceType == RGBDeviceType.Keypad)
                            geometry = CreateKeyCapGeometry(led);
                        else
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

            LayerGeometry = layerGeometry;
            LayerGeometry.Freeze();
        }

        private Geometry CreateRectangleGeometry(ArtemisLed led)
        {
            return new RectangleGeometry(led.RgbLed.AbsoluteLedRectangle.ToWindowsRect(1));
        }

        private Geometry CreateCircleGeometry(ArtemisLed led)
        {
            return new EllipseGeometry(led.RgbLed.AbsoluteLedRectangle.ToWindowsRect(1));
        }

        private Geometry CreateKeyCapGeometry(ArtemisLed led)
        {
            return new RectangleGeometry(led.RgbLed.AbsoluteLedRectangle.ToWindowsRect(1), 1.6, 1.6);
        }

        private Geometry CreateCustomGeometry(ArtemisLed led, double deflateAmount)
        {
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
                            new ScaleTransform(led.RgbLed.ActualSize.Width - deflateAmount, led.RgbLed.ActualSize.Height - deflateAmount),
                            new TranslateTransform(deflateAmount / 2, deflateAmount / 2)
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
    }
}