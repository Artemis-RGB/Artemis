using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using RGB.NET.Core;

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
            _canvas?.Children.Clear();
            _canvas = (Canvas)GetTemplateChild(PART_CANVAS);
            UpdateSurface();
        }

        private void UpdateSurface()
        {
            if ((_canvas == null) || (_surface == null)) return;

            if (_newDevices.Count > 0)
            {
                foreach (IRGBDevice device in _newDevices)
                    _canvas.Children.Add(new RGBDeviceVisualizer { Device = device });
                _newDevices.Clear();
            }

            _canvas.Width = _surface.SurfaceRectangle.Size.Width;
            _canvas.Height = _surface.SurfaceRectangle.Size.Height;
        }

        #endregion
    }
}
