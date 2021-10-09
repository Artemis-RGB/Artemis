using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Artemis.Core;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Threading;

namespace Artemis.UI.Avalonia.Shared.Controls
{
    /// <summary>
    ///     Visualizes an <see cref="ArtemisDevice" /> with optional per-LED colors
    /// </summary>
    public class DeviceVisualizer : Control
    {
        #region Properties

        /// <summary>
        ///     Gets or sets the <see cref="ArtemisDevice" /> to display
        /// </summary>
        public static readonly StyledProperty<ArtemisDevice?> DeviceProperty =
            AvaloniaProperty.Register<ProfileConfigurationIcon, ArtemisDevice?>(nameof(Device));

        /// <summary>
        ///     Gets or sets the <see cref="ArtemisDevice" /> to display
        /// </summary>
        public ArtemisDevice? Device
        {
            get => GetValue(DeviceProperty);
            set => SetValue(DeviceProperty, value);
        }

        /// <summary>
        ///     Gets or sets boolean indicating  whether or not to show per-LED colors
        /// </summary>
        public static readonly StyledProperty<bool> ShowColorsProperty =
            AvaloniaProperty.Register<ProfileConfigurationIcon, bool>(nameof(ShowColors));

        /// <summary>
        ///     Gets or sets a boolean indicating whether or not to show per-LED colors
        /// </summary>
        public bool ShowColors
        {
            get => GetValue(ShowColorsProperty);
            set => SetValue(ShowColorsProperty, value);
        }

        /// <summary>
        ///     Gets or sets a list of LEDs to highlight
        /// </summary>
        public static readonly StyledProperty<ObservableCollection<ArtemisLed>?> HighlightedLedsProperty =
            AvaloniaProperty.Register<ProfileConfigurationIcon, ObservableCollection<ArtemisLed>?>(nameof(HighlightedLeds));

        /// <summary>
        ///     Gets or sets a list of LEDs to highlight
        /// </summary>
        public ObservableCollection<ArtemisLed>? HighlightedLeds
        {
            get => GetValue(HighlightedLedsProperty);
            set => SetValue(HighlightedLedsProperty, value);
        }

        #endregion

        private readonly DispatcherTimer _timer;

        /// <inheritdoc />
        public DeviceVisualizer()
        {
            // Run an update timer at 25 fps
            _timer = new DispatcherTimer(DispatcherPriority.Render) {Interval = TimeSpan.FromMilliseconds(40)};
        }

        /// <inheritdoc />
        public override void Render(DrawingContext context)
        {
            base.Render(context);
        }

        private void Update()
        {
            throw new NotImplementedException();
        }

        #region Lifetime management

        /// <inheritdoc />
        protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            _timer.Start();
            _timer.Tick += TimerOnTick;
            base.OnAttachedToLogicalTree(e);
        }

        /// <inheritdoc />
        protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            _timer.Stop();
            _timer.Tick -= TimerOnTick;
            base.OnDetachedFromLogicalTree(e);
        }

        #endregion

        #region Event handlers

        private void TimerOnTick(object? sender, EventArgs e)
        {
            if (ShowColors && IsVisible && Opacity > 0)
                Update();
        }

        #endregion
    }
}