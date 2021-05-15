using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RGB.NET.Core;
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
        /// <param name="source">The source from where this layout is being loaded</param>
        public ArtemisLayout(string filePath, LayoutSource source)
        {
            FilePath = filePath;
            Source = source;
            Leds = new List<ArtemisLedLayout>();

            LoadLayout();
        }

        /// <summary>
        ///     Gets the file path the layout was (attempted to be) loaded from
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        ///     Gets the source from where this layout was loaded
        /// </summary>
        public LayoutSource Source { get; }

        /// <summary>
        ///     Gets the device this layout is applied to
        /// </summary>
        public ArtemisDevice? Device { get; private set; }

        /// <summary>
        ///     Gets a boolean indicating whether a valid layout was loaded
        /// </summary>
        public bool IsValid { get; private set; }

        /// <summary>
        ///     Gets the image of the device
        /// </summary>
        public Uri? Image { get; private set; }

        /// <summary>
        ///     Gets a list of LEDs this layout contains
        /// </summary>
        public List<ArtemisLedLayout> Leds { get; }

        /// <summary>
        ///     Gets the RGB.NET device layout
        /// </summary>
        public DeviceLayout RgbLayout { get; private set; } = null!;

        /// <summary>
        ///     Gets the custom layout data embedded in the RGB.NET layout
        /// </summary>
        public LayoutCustomDeviceData LayoutCustomDeviceData { get; private set; } = null!;

        /// <summary>
        ///     Applies the layout to the provided device
        /// </summary>
        public void ApplyTo(IRGBDevice device, bool createMissingLeds = false, bool removeExcessiveLeds = false)
        {
            device.Size = new Size(MathF.Round(RgbLayout.Width), MathF.Round(RgbLayout.Height));
            device.DeviceInfo.LayoutMetadata = RgbLayout.CustomData;

            HashSet<LedId> ledIds = new();
            foreach (ILedLayout layoutLed in RgbLayout.Leds)
            {
                if (Enum.TryParse(layoutLed.Id, true, out LedId ledId))
                {
                    ledIds.Add(ledId);

                    Led? led = device[ledId];
                    if (led == null && createMissingLeds)
                        led = device.AddLed(ledId, new Point(), new Size());

                    if (led != null)
                    {
                        led.Location = new Point(MathF.Round(layoutLed.X), MathF.Round(layoutLed.Y));
                        led.Size = new Size(MathF.Round(layoutLed.Width), MathF.Round(layoutLed.Height));
                        led.Shape = layoutLed.Shape;
                        led.ShapeData = layoutLed.ShapeData;
                        led.LayoutMetadata = layoutLed.CustomData;
                    }
                }
            }

            if (removeExcessiveLeds)
            {
                List<LedId> ledsToRemove = device.Select(led => led.Id).Where(id => !ledIds.Contains(id)).ToList();
                foreach (LedId led in ledsToRemove)
                    device.RemoveLed(led);
            }
        }

        internal void ApplyDevice(ArtemisDevice artemisDevice)
        {
            Device = artemisDevice;
            foreach (ArtemisLedLayout artemisLedLayout in Leds)
                artemisLedLayout.ApplyDevice(Device);
        }

        private void LoadLayout()
        {
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

        private void ApplyCustomDeviceData()
        {
            if (!IsValid)
            {
                Image = null;
                return;
            }

            Uri layoutDirectory = new(Path.GetDirectoryName(FilePath)! + "/", UriKind.Absolute);
            if (LayoutCustomDeviceData.DeviceImage != null)
                Image = new Uri(layoutDirectory, new Uri(LayoutCustomDeviceData.DeviceImage, UriKind.Relative));
            else
                Image = null;
        }
    }

    /// <summary>
    ///     Represents a source from where a layout came
    /// </summary>
    public enum LayoutSource
    {
        /// <summary>
        ///     A layout loaded from config
        /// </summary>
        Configured,

        /// <summary>
        ///     A layout loaded from the user layout folder
        /// </summary>
        User,

        /// <summary>
        ///     A layout loaded from the plugin folder
        /// </summary>
        Plugin,

        /// <summary>
        ///     A default layout loaded as a fallback option
        /// </summary>
        Default
    }
}