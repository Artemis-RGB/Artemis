using System.Windows;
using System.Windows.Controls;
using RGB.NET.Core;

namespace Artemis.UI.Controls.Visualizers
{
    /// <inheritdoc />
    /// <summary>
    ///     Visualizes a <see cref="T:RGB.NET.Core.IRGBDevice" /> in an wpf-application.
    /// </summary>
    [TemplatePart(Name = PART_CANVAS, Type = typeof(Canvas))]
    public class RGBDeviceVisualizer : Control
    {
        #region Constants

        private const string PART_CANVAS = "PART_Canvas";

        #endregion

        #region Properties & Fields

        public Canvas Canvas { get; private set; }

        #endregion

        #region DependencyProperties

        // ReSharper disable InconsistentNaming

        /// <summary>
        ///     Backing-property for the <see cref="Device" />-property.
        /// </summary>
        public static readonly DependencyProperty DeviceProperty = DependencyProperty.Register(
            "Device", typeof(IRGBDevice), typeof(RGBDeviceVisualizer), new PropertyMetadata(default(IRGBDevice), DeviceChanged));

        /// <summary>
        ///     Gets or sets the <see cref="IRGBDevice" /> to visualize.
        /// </summary>
        public IRGBDevice Device
        {
            get => (IRGBDevice) GetValue(DeviceProperty);
            set => SetValue(DeviceProperty, value);
        }

        // ReSharper restore InconsistentNaming

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void OnApplyTemplate()
        {
            Canvas = (Canvas) GetTemplateChild(PART_CANVAS);

            LayoutLeds();
        }

        private static void DeviceChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            ((RGBDeviceVisualizer) dependencyObject).LayoutLeds();
        }

        private void LayoutLeds()
        {
            if (Canvas == null) return;

            Canvas.Children.Clear();

            if (Device == null) return;

            foreach (var led in Device)
                Canvas.Children.Add(new LedVisualizer {Led = led});
        }

        #endregion
    }
}