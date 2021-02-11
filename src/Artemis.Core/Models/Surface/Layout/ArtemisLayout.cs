using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RGB.NET.Layout;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a device layout decorated with extra Artemis-specific data
    /// </summary>
    public class ArtemisLayout
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="ArtemisLayout" /> class
        /// </summary>
        /// <param name="filePath">The path of the layout XML file</param>
        public ArtemisLayout(string filePath)
        {
            FilePath = filePath;
            Leds = new List<ArtemisLedLayout>();

            DeviceLayout? deviceLayout = DeviceLayout.Load(FilePath, typeof(LayoutCustomDeviceData), typeof(LayoutCustomLedData));
            if (deviceLayout != null)
            {
                RgbLayout = deviceLayout;
                IsValid = true;
            }
            else
            {
                RgbLayout = new DeviceLayout();
                IsValid = false;
            }

            if (IsValid)
                Leds.AddRange(RgbLayout.Leds.Select(l => new ArtemisLedLayout(this, l)));
            
            LayoutCustomDeviceData = (LayoutCustomDeviceData?) RgbLayout.CustomData ?? new LayoutCustomDeviceData();
            ApplyCustomDeviceData();
        }
        
        /// <summary>
        ///     Gets the file path the layout was (attempted to be) loaded from
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        ///     Gets the RGB.NET device layout
        /// </summary>
        public DeviceLayout RgbLayout { get; }

        /// <summary>
        ///     Gets the device this layout is applied to
        /// </summary>
        public ArtemisDevice? Device { get; private set; }

        /// <summary>
        ///     Gets a boolean indicating whether a valid layout was loaded
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        ///     Gets or sets the image of the device
        /// </summary>
        public Uri? Image { get; set; }

        public List<ArtemisLedLayout> Leds { get; }

        internal LayoutCustomDeviceData LayoutCustomDeviceData { get; set; }

        internal void ApplyDevice(ArtemisDevice artemisDevice)
        {
            Device = artemisDevice;
            foreach (ArtemisLedLayout artemisLedLayout in Leds) 
                artemisLedLayout.ApplyDevice(Device);
        }

        private void ApplyCustomDeviceData()
        {
            Uri layoutDirectory = new(Path.GetDirectoryName(FilePath)! + "\\", UriKind.Absolute);
            if (LayoutCustomDeviceData.DeviceImage != null)
                Image = new Uri(layoutDirectory, new Uri(LayoutCustomDeviceData.DeviceImage, UriKind.Relative));
        }
    }
}