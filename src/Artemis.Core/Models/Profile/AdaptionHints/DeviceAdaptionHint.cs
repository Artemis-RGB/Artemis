﻿using System.Collections.Generic;
using System.Linq;
using Artemis.Storage.Entities.Profile.AdaptionHints;
using RGB.NET.Core;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a hint that adapts layers to a certain type of devices
    /// </summary>
    public class DeviceAdaptionHint : IAdaptionHint
    {
        public DeviceAdaptionHint()
        {
        }

        internal DeviceAdaptionHint(DeviceAdaptionHintEntity entity)
        {
            DeviceType = (RGBDeviceType) entity.DeviceType;
            Skip = entity.Skip;
            LimitAmount = entity.LimitAmount;
            Amount = entity.Amount;
        }

        /// <summary>
        ///     Gets or sets the type of devices LEDs will be applied to
        /// </summary>
        public RGBDeviceType DeviceType { get; set; }

        /// <summary>
        ///     Gets or sets the amount of devices to skip
        /// </summary>
        public int Skip { get; set; }

        /// <summary>
        ///     Gets or sets a boolean indicating whether a limited amount of devices should be used
        /// </summary>
        public bool LimitAmount { get; set; }

        /// <summary>
        ///     Gets or sets the amount of devices to limit to if <see cref="LimitAmount" /> is <see langword="true" />
        /// </summary>
        public int Amount { get; set; }

        #region Implementation of IAdaptionHint

        /// <inheritdoc />
        public void Apply(Layer layer, List<ArtemisDevice> devices)
        {
            IEnumerable<ArtemisDevice> matches = devices
                .Where(d => d.RgbDevice.DeviceInfo.DeviceType == DeviceType)
                .OrderBy(d => d.Rectangle)
                .Skip(Skip);
            if (LimitAmount)
                matches = matches.Take(Amount);

            foreach (ArtemisDevice artemisDevice in matches)
                layer.AddLeds(artemisDevice.Leds);
        }

        /// <inheritdoc />
        public IAdaptionHintEntity GetEntry()
        {
            return new DeviceAdaptionHintEntity {Amount = Amount, LimitAmount = LimitAmount, DeviceType = (int) DeviceType, Skip = Skip};
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Device adaption - {nameof(DeviceType)}: {DeviceType}, {nameof(Skip)}: {Skip}, {nameof(LimitAmount)}: {LimitAmount}, {nameof(Amount)}: {Amount}";
        }

        #endregion
    }
}