using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core.DeviceProviders;
using Artemis.Storage.Entities.Surface;
using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents an RGB device usable by Artemis, provided by a <see cref="DeviceProviders.DeviceProvider" />
    /// </summary>
    public class ArtemisDevice : CorePropertyChanged
    {
        private SKPath? _renderPath;
        private SKRect _renderRectangle;

        internal ArtemisDevice(IRGBDevice rgbDevice, DeviceProvider deviceProvider, ArtemisSurface surface)
        {
            DeviceEntity = new DeviceEntity();
            RgbDevice = rgbDevice;
            DeviceProvider = deviceProvider;
            Surface = surface;

            Rotation = 0;
            Scale = 1;
            ZIndex = 1;
            RedScale = 1;
            GreenScale = 1;
            BlueScale = 1;

            deviceProvider.DeviceLayoutPaths.TryGetValue(rgbDevice, out string? layoutPath);
            LayoutPath = layoutPath;

            InputIdentifiers = new List<ArtemisDeviceInputIdentifier>();

            Leds = rgbDevice.Select(l => new ArtemisLed(l, this)).ToList().AsReadOnly();
            LedIds = new ReadOnlyDictionary<LedId, ArtemisLed>(Leds.ToDictionary(l => l.RgbLed.Id, l => l));
            ApplyToEntity();
            CalculateRenderProperties();
        }

        internal ArtemisDevice(IRGBDevice rgbDevice, DeviceProvider deviceProvider, ArtemisSurface surface, DeviceEntity deviceEntity)
        {
            DeviceEntity = deviceEntity;
            RgbDevice = rgbDevice;
            DeviceProvider = deviceProvider;
            Surface = surface;

            deviceProvider.DeviceLayoutPaths.TryGetValue(rgbDevice, out string? layoutPath);
            LayoutPath = layoutPath;

            InputIdentifiers = new List<ArtemisDeviceInputIdentifier>();
            foreach (DeviceInputIdentifierEntity identifierEntity in DeviceEntity.InputIdentifiers)
                InputIdentifiers.Add(new ArtemisDeviceInputIdentifier(identifierEntity.InputProvider, identifierEntity.Identifier));

            Leds = rgbDevice.Select(l => new ArtemisLed(l, this)).ToList().AsReadOnly();
            LedIds = new ReadOnlyDictionary<LedId, ArtemisLed>(Leds.ToDictionary(l => l.RgbLed.Id, l => l));
        }

        /// <summary>
        ///     Gets the rectangle covering the device, sized to match the render scale
        /// </summary>
        public SKRect RenderRectangle
        {
            get => _renderRectangle;
            private set => SetAndNotify(ref _renderRectangle, value);
        }

        /// <summary>
        ///     Gets the path surrounding the device, sized to match the render scale
        /// </summary>
        public SKPath? RenderPath
        {
            get => _renderPath;
            private set => SetAndNotify(ref _renderPath, value);
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
        ///     Gets the surface containing this device
        /// </summary>
        public ArtemisSurface Surface { get; }

        /// <summary>
        ///     Gets a read only collection containing the LEDs of this device
        /// </summary>
        public ReadOnlyCollection<ArtemisLed> Leds { get; }

        /// <summary>
        ///     Gets a dictionary containing all the LEDs of this device with their corresponding RGB.NET <see cref="LedId" /> as
        ///     key
        /// </summary>
        public ReadOnlyDictionary<LedId, ArtemisLed> LedIds { get; }

        /// <summary>
        ///     Gets a list of input identifiers associated with this device
        /// </summary>
        public List<ArtemisDeviceInputIdentifier> InputIdentifiers { get; }

        /// <summary>
        ///     Gets or sets the X-position of the device
        /// </summary>
        public double X
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
        public double Y
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
        public double Rotation
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
        public double Scale
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

        public double RedScale
        {
            get => DeviceEntity.RedScale;
            set
            {
                DeviceEntity.RedScale = value;
                OnPropertyChanged(nameof(RedScale));
            }
        }

        public double GreenScale
        {
            get => DeviceEntity.GreenScale;
            set
            {
                DeviceEntity.GreenScale = value;
                OnPropertyChanged(nameof(GreenScale));
            }
        }

        public double BlueScale
        {
            get => DeviceEntity.BlueScale;
            set
            {
                DeviceEntity.BlueScale = value;
                OnPropertyChanged(nameof(BlueScale));
            }
        }

        /// <summary>
        ///     Gets the path to where the layout of the device was (attempted to be) loaded from
        /// </summary>
        public string? LayoutPath { get; internal set; }

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

        internal void ApplyToEntity()
        {
            // Other properties are computed
            DeviceEntity.DeviceIdentifier = RgbDevice.GetDeviceIdentifier();

            DeviceEntity.InputIdentifiers.Clear();
            foreach (ArtemisDeviceInputIdentifier identifier in InputIdentifiers)
                DeviceEntity.InputIdentifiers.Add(new DeviceInputIdentifierEntity
                {
                    InputProvider = identifier.InputProvider,
                    Identifier = identifier.Identifier
                });
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

            CalculateRenderProperties();
            OnDeviceUpdated();
        }

        internal void CalculateRenderProperties()
        {
            RenderRectangle = SKRect.Create(
                (RgbDevice.Location.X * Surface.Scale).RoundToInt(),
                (RgbDevice.Location.Y * Surface.Scale).RoundToInt(),
                (RgbDevice.DeviceRectangle.Size.Width * Surface.Scale).RoundToInt(),
                (RgbDevice.DeviceRectangle.Size.Height * Surface.Scale).RoundToInt()
            );

            if (!Leds.Any())
                return;

            foreach (ArtemisLed led in Leds)
                led.CalculateRenderRectangle();

            SKPath path = new SKPath {FillType = SKPathFillType.Winding};
            foreach (ArtemisLed artemisLed in Leds)
                path.AddRect(artemisLed.AbsoluteRenderRectangle);

            RenderPath = path;
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