using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core.DeviceProviders;
using Artemis.Core.Services;
using Artemis.Storage.Entities.Surface;
using RGB.NET.Core;
using RGB.NET.Layout;
using SkiaSharp;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents an RGB device usable by Artemis, provided by a <see cref="DeviceProviders.DeviceProvider" />
    /// </summary>
    public class ArtemisDevice : CorePropertyChanged
    {
        private SKPath? _path;
        private SKRect _rectangle;

        internal ArtemisDevice(IRGBDevice rgbDevice, DeviceProvider deviceProvider)
        {
            Identifier = rgbDevice.GetDeviceIdentifier();
            DeviceEntity = new DeviceEntity();
            RgbDevice = rgbDevice;
            DeviceProvider = deviceProvider;

            Rotation = 0;
            Scale = 1;
            ZIndex = 1;
            RedScale = 1;
            GreenScale = 1;
            BlueScale = 1;
            IsEnabled = true;

            InputIdentifiers = new List<ArtemisDeviceInputIdentifier>();

            Leds = rgbDevice.Select(l => new ArtemisLed(l, this)).ToList().AsReadOnly();
            LedIds = new ReadOnlyDictionary<LedId, ArtemisLed>(Leds.ToDictionary(l => l.RgbLed.Id, l => l));

            ApplyKeyboardLayout();
            ApplyToEntity();
            CalculateRenderProperties();
        }

        internal ArtemisDevice(IRGBDevice rgbDevice, DeviceProvider deviceProvider, DeviceEntity deviceEntity)
        {
            Identifier = rgbDevice.GetDeviceIdentifier();
            DeviceEntity = deviceEntity;
            RgbDevice = rgbDevice;
            DeviceProvider = deviceProvider;

            InputIdentifiers = new List<ArtemisDeviceInputIdentifier>();
            foreach (DeviceInputIdentifierEntity identifierEntity in DeviceEntity.InputIdentifiers)
                InputIdentifiers.Add(new ArtemisDeviceInputIdentifier(identifierEntity.InputProvider, identifierEntity.Identifier));

            Leds = rgbDevice.Select(l => new ArtemisLed(l, this)).ToList().AsReadOnly();
            LedIds = new ReadOnlyDictionary<LedId, ArtemisLed>(Leds.ToDictionary(l => l.RgbLed.Id, l => l));

            ApplyKeyboardLayout();
        }

        /// <summary>
        ///     Gets the (hopefully unique and persistent) ID of this device
        /// </summary>
        public string Identifier { get; }

        /// <summary>
        ///     Gets the rectangle covering the device
        /// </summary>
        public SKRect Rectangle
        {
            get => _rectangle;
            private set => SetAndNotify(ref _rectangle, value);
        }

        /// <summary>
        ///     Gets the path surrounding the device
        /// </summary>
        public SKPath? Path
        {
            get => _path;
            private set => SetAndNotify(ref _path, value);
        }

        /// <summary>
        ///     Gets the RGB.NET device backing this Artemis device
        /// </summary>
        public IRGBDevice RgbDevice { get; }

        /// <summary>
        ///     Gets the device provider that provided this device
        /// </summary>
        public DeviceProvider DeviceProvider { get; }

        /// <summary>
        ///     Gets a read only collection containing the LEDs of this device
        /// </summary>
        public ReadOnlyCollection<ArtemisLed> Leds { get; private set; }

        /// <summary>
        ///     Gets a dictionary containing all the LEDs of this device with their corresponding RGB.NET <see cref="LedId" /> as
        ///     key
        /// </summary>
        public ReadOnlyDictionary<LedId, ArtemisLed> LedIds { get; private set; }

        /// <summary>
        ///     Gets a list of input identifiers associated with this device
        /// </summary>
        public List<ArtemisDeviceInputIdentifier> InputIdentifiers { get; }

        /// <summary>
        ///     Gets or sets the X-position of the device
        /// </summary>
        public float X
        {
            get => DeviceEntity.X;
            set
            {
                DeviceEntity.X = value;
                OnPropertyChanged(nameof(X));
            }
        }

        /// <summary>
        ///     Gets or sets the Y-position of the device
        /// </summary>
        public float Y
        {
            get => DeviceEntity.Y;
            set
            {
                DeviceEntity.Y = value;
                OnPropertyChanged(nameof(Y));
            }
        }

        /// <summary>
        ///     Gets or sets the rotation of the device
        /// </summary>
        public float Rotation
        {
            get => DeviceEntity.Rotation;
            set
            {
                DeviceEntity.Rotation = value;
                OnPropertyChanged(nameof(Rotation));
            }
        }

        /// <summary>
        ///     Gets or sets the scale of the device
        /// </summary>
        public float Scale
        {
            get => DeviceEntity.Scale;
            set
            {
                DeviceEntity.Scale = value;
                OnPropertyChanged(nameof(Scale));
            }
        }

        /// <summary>
        ///     Gets or sets the Z-index of the device
        /// </summary>
        public int ZIndex
        {
            get => DeviceEntity.ZIndex;
            set
            {
                DeviceEntity.ZIndex = value;
                OnPropertyChanged(nameof(ZIndex));
            }
        }

        /// <summary>
        ///     Gets or sets the scale of the red color component used for calibration
        /// </summary>
        public float RedScale
        {
            get => DeviceEntity.RedScale;
            set
            {
                DeviceEntity.RedScale = value;
                OnPropertyChanged(nameof(RedScale));
            }
        }

        /// <summary>
        ///     Gets or sets the scale of the green color component used for calibration
        /// </summary>
        public float GreenScale
        {
            get => DeviceEntity.GreenScale;
            set
            {
                DeviceEntity.GreenScale = value;
                OnPropertyChanged(nameof(GreenScale));
            }
        }

        /// <summary>
        ///     Gets or sets the scale of the blue color component used for calibration
        /// </summary>
        public float BlueScale
        {
            get => DeviceEntity.BlueScale;
            set
            {
                DeviceEntity.BlueScale = value;
                OnPropertyChanged(nameof(BlueScale));
            }
        }

        /// <summary>
        ///     Gets a boolean indicating whether this devices is enabled or not
        ///     <para>Note: To enable/disable a device use the methods provided by <see cref="IRgbService" /></para>
        /// </summary>
        public bool IsEnabled
        {
            get => DeviceEntity.IsEnabled;
            internal set
            {
                DeviceEntity.IsEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
            }
        }

        /// <summary>
        ///     Gets or sets the physical layout of the device e.g. ISO or ANSI.
        ///     <para>Only applicable to keyboards</para>
        /// </summary>
        public KeyboardLayoutType PhysicalLayout
        {
            get => (KeyboardLayoutType) DeviceEntity.PhysicalLayout;
            set
            {
                DeviceEntity.PhysicalLayout = (int) value;
                OnPropertyChanged(nameof(PhysicalLayout));
            }
        }

        /// <summary>
        ///     Gets or sets the logical layout of the device e.g. DE, UK or US.
        ///     <para>Only applicable to keyboards</para>
        /// </summary>
        public string? LogicalLayout
        {
            get => DeviceEntity.LogicalLayout;
            set
            {
                DeviceEntity.LogicalLayout = value;
                OnPropertyChanged(nameof(LogicalLayout));
            }
        }

        /// <summary>
        ///     Gets or sets the path of the custom layout to load when calling <see cref="IRgbService.ApplyBestDeviceLayout" />
        ///     for this device
        /// </summary>
        public string? CustomLayoutPath
        {
            get => DeviceEntity.CustomLayoutPath;
            set
            {
                DeviceEntity.CustomLayoutPath = value;
                OnPropertyChanged(nameof(CustomLayoutPath));
            }
        }

        /// <summary>
        ///     Gets the layout of the device expanded with Artemis-specific data
        /// </summary>
        public ArtemisLayout? Layout { get; internal set; }

        internal DeviceEntity DeviceEntity { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{RgbDevice.DeviceInfo.DeviceType}] {RgbDevice.DeviceInfo.DeviceName} - {X}.{Y}.{ZIndex}";
        }

        /// <summary>
        ///     Attempts to retrieve the <see cref="ArtemisLed" /> that corresponds the provided RGB.NET <see cref="Led" />
        /// </summary>
        /// <param name="led">The RGB.NET <see cref="Led" /> to find the corresponding <see cref="ArtemisLed" /> for </param>
        /// <returns>If found, the corresponding <see cref="ArtemisLed" />; otherwise <see langword="null" />.</returns>
        public ArtemisLed? GetLed(Led led)
        {
            return GetLed(led.Id);
        }

        /// <summary>
        ///     Attempts to retrieve the <see cref="ArtemisLed" /> that corresponds the provided RGB.NET <see cref="LedId" />
        /// </summary>
        /// <param name="ledId">The RGB.NET <see cref="LedId" /> to find the corresponding <see cref="ArtemisLed" /> for </param>
        /// <returns>If found, the corresponding <see cref="ArtemisLed" />; otherwise <see langword="null" />.</returns>
        public ArtemisLed? GetLed(LedId ledId)
        {
            LedIds.TryGetValue(ledId, out ArtemisLed? artemisLed);
            return artemisLed;
        }

        /// <summary>
        ///     Generates the default layout file name of the device
        /// </summary>
        /// <param name="includeExtension">If true, the .xml extension is added to the file name</param>
        /// <returns>The resulting file name e.g. CORSAIR GLAIVE.xml or K95 RGB-ISO.xml</returns>
        public string GetLayoutFileName(bool includeExtension = true)
        {
            // Take out invalid file name chars, may not be perfect but neither are you
            string fileName = System.IO.Path.GetInvalidFileNameChars().Aggregate(RgbDevice.DeviceInfo.Model, (current, c) => current.Replace(c, '-'));
            if (RgbDevice.DeviceInfo.DeviceType == RGBDeviceType.Keyboard)
                fileName = $"{fileName}-{PhysicalLayout.ToString().ToUpper()}";
            if (includeExtension)
                fileName = $"{fileName}.xml";

            return fileName;
        }

        /// <summary>
        ///     Applies the provided layout to the device
        /// </summary>
        /// <param name="layout">The layout to apply</param>
        /// <param name="createMissingLeds">A boolean indicating whether to add missing LEDs defined in the layout but missing on the device</param>
        /// <param name="removeExcessiveLeds">A boolean indicating whether to remove excess LEDs present in the device but missing in the layout</param>
        internal void ApplyLayout(ArtemisLayout layout, bool createMissingLeds, bool removeExcessiveLeds)
        {
            if (layout.IsValid)
                layout.RgbLayout!.ApplyTo(RgbDevice, createMissingLeds, removeExcessiveLeds);

            Leds = RgbDevice.Select(l => new ArtemisLed(l, this)).ToList().AsReadOnly();
            LedIds = new ReadOnlyDictionary<LedId, ArtemisLed>(Leds.ToDictionary(l => l.RgbLed.Id, l => l));

            Layout = layout;
            Layout.ApplyDevice(this);
            OnDeviceUpdated();
        }

        internal void ApplyToEntity()
        {
            // Other properties are computed
            DeviceEntity.Id = Identifier;

            DeviceEntity.InputIdentifiers.Clear();
            foreach (ArtemisDeviceInputIdentifier identifier in InputIdentifiers)
            {
                DeviceEntity.InputIdentifiers.Add(new DeviceInputIdentifierEntity
                {
                    InputProvider = identifier.InputProvider,
                    Identifier = identifier.Identifier
                });
            }
        }

        internal void ApplyToRgbDevice()
        {
            RgbDevice.Rotation = DeviceEntity.Rotation;
            RgbDevice.Scale = DeviceEntity.Scale;

            // Workaround for device rotation not applying
            if (DeviceEntity.X == 0 && DeviceEntity.Y == 0)
                RgbDevice.Location = new Point(1, 1);
            RgbDevice.Location = new Point(DeviceEntity.X, DeviceEntity.Y);

            InputIdentifiers.Clear();
            foreach (DeviceInputIdentifierEntity identifierEntity in DeviceEntity.InputIdentifiers)
                InputIdentifiers.Add(new ArtemisDeviceInputIdentifier(identifierEntity.InputProvider, identifierEntity.Identifier));

            if (!RgbDevice.ColorCorrections.Any())
                RgbDevice.ColorCorrections.Add(new ScaleColorCorrection(this));

            CalculateRenderProperties();
            OnDeviceUpdated();
        }

        internal void CalculateRenderProperties()
        {
            Rectangle = RgbDevice.Boundary.ToSKRect();
            if (!Leds.Any())
                return;

            foreach (ArtemisLed led in Leds)
                led.CalculateRectangles();

            SKPath path = new() {FillType = SKPathFillType.Winding};
            foreach (ArtemisLed artemisLed in Leds)
                path.AddRect(artemisLed.AbsoluteRectangle);

            Path = path;
        }

        private void ApplyKeyboardLayout()
        {
            if (RgbDevice.DeviceInfo.DeviceType != RGBDeviceType.Keyboard)
                return;

            IKeyboard? keyboard = RgbDevice as IKeyboard;
            // If supported, detect the device layout so that we can load the correct one
            if (DeviceProvider.CanDetectPhysicalLayout && keyboard != null)
                PhysicalLayout = (KeyboardLayoutType) keyboard.DeviceInfo.Layout;
            else
                PhysicalLayout = (KeyboardLayoutType) DeviceEntity.PhysicalLayout;
            if (DeviceProvider.CanDetectLogicalLayout && keyboard != null)
                LogicalLayout = DeviceProvider.GetLogicalLayout(keyboard);
            else
                LogicalLayout = DeviceEntity.LogicalLayout;
        }

        #region Events

        /// <summary>
        ///     Occurs when the underlying RGB.NET device was updated
        /// </summary>
        public event EventHandler? DeviceUpdated;

        /// <summary>
        ///     Invokes the <see cref="DeviceUpdated" /> event
        /// </summary>
        protected virtual void OnDeviceUpdated()
        {
            DeviceUpdated?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}