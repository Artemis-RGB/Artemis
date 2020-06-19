using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.LayerShapes;
using Artemis.Core.Models.Surface;
using Artemis.UI.Extensions;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Shared.Services.Interfaces;
using RGB.NET.Core;
using Stylet;
using Rectangle = Artemis.Core.Models.Profile.LayerShapes.Rectangle;

namespace Artemis.UI.Screens.Module.ProfileEditor.Visualization
{
    public class ProfileLayerViewModel : CanvasViewModel
    {
        private readonly ILayerEditorService _layerEditorService;
        private readonly IProfileEditorService _profileEditorService;

        public ProfileLayerViewModel(Layer layer, IProfileEditorService profileEditorService, ILayerEditorService layerEditorService)
        {
            _profileEditorService = profileEditorService;
            _layerEditorService = layerEditorService;
            Layer = layer;

            Update();
            Layer.RenderPropertiesUpdated += LayerOnRenderPropertiesUpdated;
            _profileEditorService.ProfileElementSelected += OnProfileElementSelected;
            _profileEditorService.SelectedProfileElementUpdated += OnSelectedProfileElementUpdated;
            _profileEditorService.ProfilePreviewUpdated += ProfileEditorServiceOnProfilePreviewUpdated;
        }

        public Layer Layer { get; }

        public Geometry LayerGeometry { get; set; }
        public Geometry OpacityGeometry { get; set; }
        public Geometry ShapeGeometry { get; set; }
        public RenderTargetBitmap LayerGeometryBitmap { get; set; }
        public Rect ViewportRectangle { get; set; }
        public Thickness LayerPosition => new Thickness(ViewportRectangle.Left, ViewportRectangle.Top, 0, 0);
        public bool IsSelected { get; set; }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Layer.RenderPropertiesUpdated -= LayerOnRenderPropertiesUpdated;
                _profileEditorService.ProfileElementSelected -= OnProfileElementSelected;
                _profileEditorService.SelectedProfileElementUpdated -= OnSelectedProfileElementUpdated;
                _profileEditorService.ProfilePreviewUpdated -= ProfileEditorServiceOnProfilePreviewUpdated;
            }

            base.Dispose(disposing);
        }
        
        private void Update()
        {
            IsSelected = _profileEditorService.SelectedProfileElement == Layer;

            CreateLayerGeometry();
            CreateShapeGeometry();
            CreateViewportRectangle();
        }

        private void CreateLayerGeometry()
        {
            Execute.OnUIThread(() =>
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

                LayerGeometry = layerGeometry;
                OpacityGeometry = opacityGeometry;

                // Render the store as a bitmap 

                var drawingImage = new DrawingImage(new GeometryDrawing(new SolidColorBrush(Colors.Black), null, LayerGeometry));
                var image = new Image {Source = drawingImage};
                var bitmap = new RenderTargetBitmap(
                    (int) (LayerGeometry.Bounds.Width * 2.5),
                    (int) (LayerGeometry.Bounds.Height * 2.5),
                    96,
                    96,
                    PixelFormats.Pbgra32
                );
                image.Arrange(new Rect(0, 0, bitmap.Width, bitmap.Height));
                bitmap.Render(image);
                bitmap.Freeze();
                LayerGeometryBitmap = bitmap;
            });
        }

        private void CreateShapeGeometry()
        {
            if (Layer.LayerShape == null || !Layer.Leds.Any())
            {
                ShapeGeometry = Geometry.Empty;
                return;
            }

            Execute.PostToUIThread(() =>
            {
                var bounds = _layerEditorService.GetLayerBounds(Layer);
                var shapeGeometry = Geometry.Empty;
                switch (Layer.LayerShape)
                {
                    case Ellipse _:
                        shapeGeometry = new EllipseGeometry(bounds);
                        break;
                    case Rectangle _:
                        shapeGeometry = new RectangleGeometry(bounds);
                        break;
                }

                if (Layer.LayerBrush == null || Layer.LayerBrush.SupportsTransformation)
                    shapeGeometry.Transform = _layerEditorService.GetLayerTransformGroup(Layer);

                ShapeGeometry = shapeGeometry;
            });
        }

        private void CreateViewportRectangle()
        {
            if (!Layer.Leds.Any() || Layer.LayerShape == null)
            {
                ViewportRectangle = Rect.Empty;
                return;
            }

            ViewportRectangle = _layerEditorService.GetLayerBounds(Layer);
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

        #region Event handlers

        private void LayerOnRenderPropertiesUpdated(object sender, EventArgs e)
        {
            Update();
        }

        private void OnProfileElementSelected(object sender, EventArgs e)
        {
            IsSelected = _profileEditorService.SelectedProfileElement == Layer;
        }

        private void OnSelectedProfileElementUpdated(object sender, EventArgs e)
        {
            Update();
        }

        private void ProfileEditorServiceOnProfilePreviewUpdated(object sender, EventArgs e)
        {
            if (!Layer.Transform.PropertiesInitialized || !Layer.General.PropertiesInitialized)
                return;

            CreateShapeGeometry();
            CreateViewportRectangle();
        }

        #endregion
    }
}