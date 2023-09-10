using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Artemis.Storage.Entities.Profile.AdaptionHints;
using RGB.NET.Core;

namespace Artemis.Core;

/// <summary>
///     Represents a hint that adapts layers to a certain region of keyboards
/// </summary>
public class KeyboardSectionAdaptionHint : CorePropertyChanged, IAdaptionHint
{
    private static readonly Dictionary<KeyboardSection, List<LedId>> RegionLedIds = new()
    {
        {KeyboardSection.MacroKeys, Enum.GetValues<LedId>().Where(l => l >= LedId.Keyboard_Programmable1 && l <= LedId.Keyboard_Programmable32).ToList()},
        {KeyboardSection.LedStrips, Enum.GetValues<LedId>().Where(l => l >= LedId.LedStripe1 && l <= LedId.LedStripe128).ToList()},
        {KeyboardSection.Extra, Enum.GetValues<LedId>().Where(l => l >= LedId.Keyboard_Custom1 && l <= LedId.Keyboard_Custom64).ToList()},
        {KeyboardSection.FunctionKeys, Enum.GetValues<LedId>().Where(l => l >= LedId.Keyboard_F1 && l <= LedId.Keyboard_F12).ToList()},
        {KeyboardSection.NumberKeys, Enum.GetValues<LedId>().Where(l => l >= LedId.Keyboard_1 && l <= LedId.Keyboard_0).ToList()},
        {KeyboardSection.NumPad, Enum.GetValues<LedId>().Where(l => l >= LedId.Keyboard_NumLock && l <= LedId.Keyboard_NumPeriodAndDelete).ToList()},
        {KeyboardSection.ArrowKeys, Enum.GetValues<LedId>().Where(l => l >= LedId.Keyboard_PageDown && l <= LedId.Keyboard_ArrowRight).ToList()},
        {KeyboardSection.MediaKeys, Enum.GetValues<LedId>().Where(l => l >= LedId.Keyboard_MediaMute && l <= LedId.Keyboard_MediaNextTrack).ToList()},
    };

    private KeyboardSection _section;

    /// <summary>
    ///     Creates a new instance of the <see cref="KeyboardSectionAdaptionHint" /> class
    /// </summary>
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
    public KeyboardSection Section
    {
        get => _section;
        set => SetAndNotify(ref _section, value);
    }

    #region Implementation of IAdaptionHint

    /// <inheritdoc />
    public void Apply(Layer layer, List<ArtemisDevice> devices)
    {
        if (Section == KeyboardSection.Movement)
        {
            ApplyMovement(layer, devices);
            return;
        }

        // Only keyboards should have the LEDs we care about
        foreach (ArtemisDevice keyboard in devices.Where(d => d.DeviceType == RGBDeviceType.Keyboard))
        {
            List<LedId> ledIds = RegionLedIds[Section];
            layer.AddLeds(keyboard.Leds.Where(l => ledIds.Contains(l.RgbLed.Id)));
        }
    }

    private void ApplyMovement(Layer layer, List<ArtemisDevice> devices)
    {
        // Only keyboards should have the LEDs we care about
        foreach (ArtemisDevice keyboard in devices.Where(d => d.DeviceType == RGBDeviceType.Keyboard))
        {
            ArtemisLed? qLed = keyboard.Leds.FirstOrDefault(l => l.RgbLed.Id == LedId.Keyboard_Q);
            ArtemisLed? aLed = keyboard.Leds.FirstOrDefault(l => l.RgbLed.Id == LedId.Keyboard_A);
            if (qLed == null || aLed == null)
                continue;

            // AZERTY keyboards will have their A above their Q
            bool isAzerty = aLed.Rectangle.MidX < qLed.Rectangle.MidX;

            if (isAzerty)
                layer.AddLeds(keyboard.Leds.Where(l => l.RgbLed.Id is LedId.Keyboard_Z or LedId.Keyboard_Q or LedId.Keyboard_S or LedId.Keyboard_D));
            else
                layer.AddLeds(keyboard.Leds.Where(l => l.RgbLed.Id is LedId.Keyboard_W or LedId.Keyboard_A or LedId.Keyboard_S or LedId.Keyboard_D));
        }
    }

    /// <inheritdoc />
    public IAdaptionHintEntity GetEntry()
    {
        return new KeyboardSectionAdaptionHintEntity {Section = (int) Section};
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
    [Description("Macro Keys")] MacroKeys,

    /// <summary>
    ///     A region containing the LED strips of a keyboard
    /// </summary>
    [Description("LED Strips")] LedStrips,

    /// <summary>
    ///     A region containing the extra non-standard LEDs of a keyboard
    /// </summary>
    [Description("Extra LEDs")] Extra,

    /// <summary>
    ///     A region containing the movement keys of a keyboard (WASD for QWERTY and ZQSD for AZERTY)
    /// </summary>
    [Description("Movement (WASD/ZQSD)")] Movement,

    /// <summary>
    ///     A region containing the F-keys of a keyboard
    /// </summary>
    [Description("F-keys")] FunctionKeys,

    /// <summary>
    ///     A region containing the numeric keys of a keyboard
    /// </summary>
    [Description("Numeric keys")] NumberKeys,

    /// <summary>
    ///     A region containing the Numpad of a keyboard
    /// </summary>
    [Description("Numpad")] NumPad,

    /// <summary>
    ///     A region containing the arrow keys of a keyboard
    /// </summary>
    [Description("Arrow keys")] ArrowKeys,

    /// <summary>
    ///     A region containing the media keys of a keyboard
    /// </summary>
    [Description("Media keys")] MediaKeys
}