using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RGB.NET.Core;

namespace Artemis.UI.Controls.Visualizers
{
    /// <inheritdoc />
    /// <summary>
    ///     Visualizes a <see cref="T:RGB.NET.Core.Led" /> in an wpf-application.
    /// </summary>
    public class LedVisualizer : Control
    {
        #region DependencyProperties

        // ReSharper disable InconsistentNaming

        /// <summary>
        ///     Backing-property for the <see cref="Led" />-property.
        /// </summary>
        public static readonly DependencyProperty LedProperty = DependencyProperty.Register(
            "Led", typeof(Led), typeof(LedVisualizer), new PropertyMetadata(default(Led)));

        /// <summary>
        ///     Gets or sets the <see cref="RGB.NET.Core.Led" /> to visualize.
        /// </summary>
        public Led Led
        {
            get => (Led) GetValue(LedProperty);
            set => SetValue(LedProperty, value);
        }

        // ReSharper restore InconsistentNaming

        #endregion

        public void Select()
        {
            BorderBrush = new SolidColorBrush(Colors.RoyalBlue);
        }

        public void Deselect()
        {
            BorderBrush = new SolidColorBrush(Colors.Black);
        }
    }
}