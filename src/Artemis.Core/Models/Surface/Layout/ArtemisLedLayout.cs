using System;
using System.IO;
using System.Linq;
using RGB.NET.Layout;

namespace Artemis.Core
{
    public class ArtemisLedLayout
    {
        internal ArtemisLedLayout(ArtemisLayout layout, ILedLayout led)
        {
            Layout = layout;
            RgbLayout = led;
            LayoutCustomLedData = (LayoutCustomLedData?) led.CustomData ?? new LayoutCustomLedData();
        }

        public ArtemisLayout Layout { get; }

        /// <summary>
        ///     Gets the RGB.NET LED Layout of this Artemis LED layout
        /// </summary>
        public ILedLayout RgbLayout { get; }

        /// <summary>
        ///     Gets the LED this layout is applied to
        /// </summary>
        public ArtemisLed? Led { get; protected set; }

        /// <summary>
        ///     Gets the name of the logical layout this LED belongs to
        /// </summary>
        public string? LogicalName { get; private set; }

        /// <summary>
        ///     Gets the image of the LED
        /// </summary>
        public Uri? Image { get; private set; }

        internal LayoutCustomLedData LayoutCustomLedData { get; set; }

        public void ApplyDevice(ArtemisDevice device)
        {
            Led = device.Leds.FirstOrDefault(d => d.RgbLed.Id.ToString() == RgbLayout.Id);
            ApplyCustomLedData();
        }

        private void ApplyCustomLedData()
        {
            if (Led == null)
                return;
            
            Uri layoutDirectory = new(Path.GetDirectoryName(Layout.FilePath)! + "\\", UriKind.Absolute);
        }
    }
}