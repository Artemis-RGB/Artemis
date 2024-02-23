using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RGB.NET.Core;
using RGB.NET.Layout;

namespace Artemis.Core;

/// <summary>
///     Represents a device layout decorated with extra Artemis-specific data
/// </summary>
public class ArtemisLayout
{
    private static readonly string DefaultLayoutPath = Path.Combine(Constants.ApplicationFolder, "DefaultLayouts", "Artemis");

    /// <summary>
    ///     Creates a new instance of the <see cref="ArtemisLayout" /> class
    /// </summary>
    /// <param name="filePath">The path of the layout XML file</param>
    public ArtemisLayout(string filePath)
    {
        FilePath = filePath;
        Leds = new List<ArtemisLedLayout>();
        IsDefaultLayout = filePath.StartsWith(DefaultLayoutPath);
        
        LoadLayout();
    }

    /// <summary>
    ///     Gets the file path the layout was (attempted to be) loaded from
    /// </summary>
    public string FilePath { get; }

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
    ///     Gets a boolean indicating whether this layout is a default layout or not
    /// </summary>
    public bool IsDefaultLayout { get; private set; }

    /// <summary>
    ///     Applies the layout to the provided device
    /// </summary>
    public void ApplyToDevice(IRGBDevice device, bool createMissingLeds = false, bool removeExcessiveLeds = false)
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
                    led.Location = new Point(layoutLed.X, layoutLed.Y);
                    led.Size = new Size(layoutLed.Width, layoutLed.Height);
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

        List<Led> deviceLeds = device.ToList();
        foreach (Led led in deviceLeds)
        {
            float x = led.Location.X;
            float y = led.Location.Y;

            // Try to move the LED if it falls outside the boundaries of the layout
            if (led.Location.X + led.Size.Width > device.Size.Width)
                x -= led.Location.X + led.Size.Width - device.Size.Width;

            if (led.Location.Y + led.Size.Height > device.Size.Height)
                y -= led.Location.Y + led.Size.Height - device.Size.Height;

            // If not possible because it's too large we'll have to drop it to avoid rendering issues
            if (x < 0 || y < 0)
                device.RemoveLed(led.Id);
            else
                led.Location = new Point(x, y);
        }
    }

    internal static ArtemisLayout? GetDefaultLayout(ArtemisDevice device)
    {
        if (device.DeviceType == RGBDeviceType.Keyboard)
        {
            // XL layout is defined by its programmable macro keys
            if (device.Leds.Any(l => l.RgbLed.Id >= LedId.Keyboard_Programmable1 && l.RgbLed.Id <= LedId.Keyboard_Programmable32))
            {
                if (device.PhysicalLayout == KeyboardLayoutType.ANSI)
                    return new ArtemisLayout(Path.Combine(DefaultLayoutPath, "Keyboard", "Artemis XL keyboard-ANSI.xml"));
                return new ArtemisLayout(Path.Combine(DefaultLayoutPath, "Keyboard", "Artemis XL keyboard-ISO.xml"));
            }

            // L layout is defined by its numpad
            if (device.Leds.Any(l => l.RgbLed.Id >= LedId.Keyboard_NumLock && l.RgbLed.Id <= LedId.Keyboard_NumPeriodAndDelete))
            {
                if (device.PhysicalLayout == KeyboardLayoutType.ANSI)
                    return new ArtemisLayout(Path.Combine(DefaultLayoutPath, "Keyboard", "Artemis L keyboard-ANSI.xml"));
                return new ArtemisLayout(Path.Combine(DefaultLayoutPath, "Keyboard", "Artemis L keyboard-ISO.xml"));
            }

            // No numpad will result in TKL
            if (device.PhysicalLayout == KeyboardLayoutType.ANSI)
                return new ArtemisLayout(Path.Combine(DefaultLayoutPath, "Keyboard", "Artemis TKL keyboard-ANSI.xml"));
            return new ArtemisLayout(Path.Combine(DefaultLayoutPath, "Keyboard", "Artemis TKL keyboard-ISO.xml"));
        }

        // if (device.DeviceType == RGBDeviceType.Mouse)
        // {
        //     if (device.Leds.Count == 1)
        //     {
        //         if (device.Leds.Any(l => l.RgbLed.Id == LedId.Logo))
        //             return new ArtemisLayout(Path.Combine(DefaultLayoutPath, "Mouse", "1 LED mouse logo.xml"), LayoutSource.Default);
        //         return new ArtemisLayout(Path.Combine(DefaultLayoutPath, "Mouse", "1 LED mouse.xml"), LayoutSource.Default);
        //     }
        //     if (device.Leds.Any(l => l.RgbLed.Id == LedId.Logo))
        //         return new ArtemisLayout(Path.Combine(DefaultLayoutPath, "Mouse", "4 LED mouse logo.xml"), LayoutSource.Default);
        //     return new ArtemisLayout(Path.Combine(DefaultLayoutPath, "Mouse", "4 LED mouse.xml"), LayoutSource.Default);
        // }

        if (device.DeviceType == RGBDeviceType.Headset)
        {
            if (device.Leds.Count == 1)
                return new ArtemisLayout(Path.Combine(DefaultLayoutPath, "Headset", "Artemis 1 LED headset.xml"));
            if (device.Leds.Count == 2)
                return new ArtemisLayout(Path.Combine(DefaultLayoutPath, "Headset", "Artemis 2 LED headset.xml"));
            return new ArtemisLayout(Path.Combine(DefaultLayoutPath, "Headset", "Artemis 4 LED headset.xml"));
        }

        return null;
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

    /// <inheritdoc />
    public override string ToString()
    {
        return FilePath;
    }
}