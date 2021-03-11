using System;
using System.IO;
using System.Linq;
using RGB.NET.Layout;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a LED layout decorated with extra Artemis-specific data
    /// </summary>
    public class ArtemisLedLayout
    {
        internal ArtemisLedLayout(ArtemisLayout deviceLayout, ILedLayout led)
        {
            DeviceLayout = deviceLayout;
            RgbLayout = led;
            LayoutCustomLedData = (LayoutCustomLedData?) led.CustomData ?? new LayoutCustomLedData();
        }

        /// <summary>
        ///     Gets the device layout of this LED layout
        /// </summary>
        public ArtemisLayout DeviceLayout { get; }

        /// <summary>
        ///     Gets the RGB.NET LED Layout of this LED layout
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

        /// <summary>
        ///     Gets the custom layout data embedded in the RGB.NET layout
        /// </summary>
        public LayoutCustomLedData LayoutCustomLedData { get; }

        internal void ApplyDevice(ArtemisDevice device)
        {
            Led = device.Leds.FirstOrDefault(d => d.RgbLed.Id.ToString() == RgbLayout.Id);
            if (Led != null) 
                Led.Layout = this;

            ApplyCustomLedData(device);
        }

        private void ApplyCustomLedData(ArtemisDevice artemisDevice)
        {
            if (LayoutCustomLedData.LogicalLayouts == null || !LayoutCustomLedData.LogicalLayouts.Any())
                return;

            Uri layoutDirectory = new(Path.GetDirectoryName(DeviceLayout.FilePath)! + "\\", UriKind.Absolute);
            // Prefer a matching layout or else a default layout (that has no name)
            LayoutCustomLedDataLogicalLayout logicalLayout = LayoutCustomLedData.LogicalLayouts
                .OrderBy(l => l.Name == artemisDevice.LogicalLayout)
                .ThenBy(l => l.Name == null)
                .First();

            LogicalName = logicalLayout.Name;
            Image = new Uri(layoutDirectory, logicalLayout.Image);
        }
    }
}