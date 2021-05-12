using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Storage.Entities.Profile.AdaptionHints;
using RGB.NET.Core;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a hint that adapts layers to a certain region of keyboards
    /// </summary>
    public class KeyboardSectionAdaptionHint : IAdaptionHint
    {
        private static readonly Dictionary<KeyboardSection, List<LedId>> RegionLedIds = new()
        {
            {KeyboardSection.MacroKeys, Enum.GetValues<LedId>().Where(l => l >= LedId.Keyboard_Programmable1 && l <= LedId.Keyboard_Programmable32).ToList()},
            {KeyboardSection.LedStrips, Enum.GetValues<LedId>().Where(l => l >= LedId.LedStripe1 && l <= LedId.LedStripe128).ToList()},
            {KeyboardSection.Extra, Enum.GetValues<LedId>().Where(l => l >= LedId.Keyboard_Custom1 && l <= LedId.Keyboard_Custom64).ToList()}
        };

        public KeyboardSectionAdaptionHint()
        {
        }

        internal KeyboardSectionAdaptionHint(KeyboardSectionAdaptionHintEntity entity)
        {
            Section = (KeyboardSection) entity.Section;
        }

        /// <summary>
        ///     Gets or sets the section this hint will apply LEDs to
        /// </summary>
        public KeyboardSection Section { get; set; }

        #region Implementation of IAdaptionHint

        /// <inheritdoc />
        public void Apply(Layer layer, List<ArtemisDevice> devices)
        {
            // Only keyboards should have the LEDs we care about
            foreach (ArtemisDevice keyboard in devices.Where(d => d.RgbDevice.DeviceInfo.DeviceType == RGBDeviceType.Keyboard))
            {
                List<LedId> ledIds = RegionLedIds[Section];
                layer.AddLeds(keyboard.Leds.Where(l => ledIds.Contains(l.RgbLed.Id)));
            }
        }

        /// <inheritdoc />
        public IAdaptionHintEntity GetEntry()
        {
            return new KeyboardSectionAdaptionHintEntity() {Section = (int) Section};
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Keyboard section adaption - {nameof(Section)}: {Section}";
        }

        #endregion
    }

    /// <summary>
    ///     Represents a section of LEDs on a keyboard
    /// </summary>
    public enum KeyboardSection
    {
        /// <summary>
        ///     A region containing the macro keys of a keyboard
        /// </summary>
        MacroKeys,

        /// <summary>
        ///     A region containing the LED strips of a keyboard
        /// </summary>
        LedStrips,

        /// <summary>
        ///     A region containing extra non-standard LEDs of a keyboard
        /// </summary>
        Extra
    }
}