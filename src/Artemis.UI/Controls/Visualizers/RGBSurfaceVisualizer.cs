using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Artemis.UI.Adorners;
using RGB.NET.Core;
using RGB.NET.Groups;
using Point = System.Windows.Point;

namespace Artemis.UI.Controls.Visualizers
{
    /// <inheritdoc />
    /// <summary>
    /// Visualizes the <see cref="T:RGB.NET.Core.RGBSurface" /> in an wpf-application.
    /// </summary>
    [TemplatePart(Name = PART_CANVAS, Type = typeof(Canvas))]
    public class RGBSurfaceVisualizer : Control
    {
        #region Constants

        private const string PART_CANVAS = "PART_Canvas";

        #endregion

        #region Properties & Fields

        private RGBSurface _surface;
        private Canvas _canvas;
        private BoundingBoxAdorner _boundingBox;
        private Point _startingPoint;

        //TODO DarthAffe 17.06.2017: This is ugly - redesign how device connect/disconnect is generally handled!
        private readonly List<IRGBDevice> _newDevices = new List<IRGBDevice>();

        #endregion

        #region Constructors

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:RGB.NET.WPF.Controls.RGBSurfaceVisualizer" /> class.
        /// </summary>
        public RGBSurfaceVisualizer()
        {
            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            _surface = RGBSurface.Instance;

            _surface.SurfaceLayoutChanged += RGBSurfaceOnSurfaceLayoutChanged;
            foreach (IRGBDevice device in _surface.Devices)
                _newDevices.Add(device);

            UpdateSurface();
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            _surface.SurfaceLayoutChanged -= RGBSurfaceOnSurfaceLayoutChanged;
            _canvas?.Children.Clear();
            _newDevices.Clear();
        }

        private void RGBSurfaceOnSurfaceLayoutChanged(SurfaceLayoutChangedEventArgs args)
        {
            if (args.DeviceAdded)
                foreach (IRGBDevice device in args.Devices)
                    _newDevices.Add(device);

            UpdateSurface();
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void OnApplyTemplate()
        {
            // Detach any existing event handlers
            if (_canvas != null)
            {
                _canvas.MouseLeftButtonDown -= ScrollViewerOnMouseLeftButtonDown;
                _canvas.MouseLeftButtonUp -= ScrollViewerOnMouseLeftButtonUp;
            }
            _canvas?.Children.Clear();
            _canvas = (Canvas) GetTemplateChild(PART_CANVAS);

            UpdateSurface();

            if (_canvas == null) return;
            _canvas.MouseLeftButtonDown += ScrollViewerOnMouseLeftButtonDown;
            _canvas.MouseLeftButtonUp += ScrollViewerOnMouseLeftButtonUp;
            _canvas.MouseMove += ScrollViewerOnMouseMove;
        }


        private void ScrollViewerOnMouseLeftButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            _canvas.CaptureMouse();
            _startingPoint = mouseButtonEventArgs.GetPosition(_canvas);
            _boundingBox = new BoundingBoxAdorner(_canvas, Colors.RoyalBlue);

            var adornerLayer = AdornerLayer.GetAdornerLayer(_canvas);
            adornerLayer.Add(_boundingBox);
        }

        private void ScrollViewerOnMouseLeftButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            _canvas.ReleaseMouseCapture();
            var adornerLayer = AdornerLayer.GetAdornerLayer(_canvas);
            var adorners = adornerLayer.GetAdorners(_canvas);
            if (adorners == null) return;
            foreach (var adorner in adorners)
                adornerLayer.Remove(adorner);

            _boundingBox = null;
        }

        private void ScrollViewerOnMouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            if (_boundingBox == null) return;
            var currentPoint = mouseEventArgs.GetPosition(_canvas);
            _boundingBox.Update(_startingPoint, currentPoint);

            var ledStart = new RGB.NET.Core.Point(_startingPoint.X, _startingPoint.Y);
            var ledEnd = new RGB.NET.Core.Point(currentPoint.X, currentPoint.Y);
            var selection = new RectangleLedGroup(ledStart, ledEnd, 0.1);

            // Deselect all LED of each device
            var deviceLeds = new List<LedVisualizer>();
            foreach (var rgbDeviceVisualizer in _canvas.Children.Cast<RGBDeviceVisualizer>())
                deviceLeds.AddRange(rgbDeviceVisualizer.Canvas.Children.Cast<LedVisualizer>());

            foreach (var ledVisualizer in deviceLeds)
                ledVisualizer?.Deselect();

            // Select all LEDs in the bounding box
            foreach (var led in selection.GetLeds())
            {
                var ledVisualizer = deviceLeds.FirstOrDefault(l => l.Led == led);
                ledVisualizer?.Select();
            }
        }

        private void UpdateSurface()
        {
            if ((_canvas == null) || (_surface == null)) return;

            if (_newDevices.Count > 0)
            {
                foreach (IRGBDevice device in _newDevices)
                    _canvas.Children.Add(new RGBDeviceVisualizer {Device = device});
                _newDevices.Clear();
            }

            _canvas.Width = _surface.SurfaceRectangle.Size.Width;
            _canvas.Height = _surface.SurfaceRectangle.Size.Height;
        }

        #endregion
    }
}