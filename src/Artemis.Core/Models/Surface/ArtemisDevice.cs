using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Artemis.Core.DeviceProviders;
using Artemis.Core.Providers;
using Artemis.Core.Services;
using Artemis.Storage.Entities.Surface;
using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.Core;

/// <summary>
///     Represents an RGB device usable by Artemis, provided by a <see cref="DeviceProviders.DeviceProvider" />
/// </summary>
public class ArtemisDevice : CorePropertyChanged
{
    private readonly List<OriginalLed> _originalLeds;
    private readonly Size _originalSize;
    private SKPath? _path;
    private SKRect _rectangle;

    internal ArtemisDevice(IRGBDevice rgbDevice, DeviceProvider deviceProvider)
    {
        rgbDevice.EnsureValidDimensions();
        _originalLeds = new List<OriginalLed>(rgbDevice.Select(l => new OriginalLed(l)));
        _originalSize = rgbDevice.Size;

        RgbDevice = rgbDevice;
        Identifier = rgbDevice.GetDeviceIdentifier();
        DeviceEntity = new DeviceEntity();
        DeviceProvider = deviceProvider;

        Rotation = 0;
        Scale = 1;
        ZIndex = 1;
        RedScale = 1;
        GreenScale = 1;
        BlueScale = 1;
        IsEnabled = true;

        LedIds = new ReadOnlyDictionary<LedId, ArtemisLed>(new Dictionary<LedId, ArtemisLed>());
        Leds = new ReadOnlyCollection<ArtemisLed>(new List<ArtemisLed>());
        InputIdentifiers = new List<ArtemisDeviceInputIdentifier>();
        InputMappings = new Dictionary<ArtemisLed, ArtemisLed>();
        Categories = new HashSet<DeviceCategory>();
        LayoutSelection = new LayoutSelection {Type = DefaultLayoutProvider.LayoutType};

        RgbDevice.ColorCorrections.Clear();
        RgbDevice.ColorCorrections.Add(new ScaleColorCorrection(this));

        CreateArtemisLeds(false);
        ApplyKeyboardLayout();
        CalculateRenderProperties();
        Save();

        RgbDevice.PropertyChanged += RgbDeviceOnPropertyChanged;
    }

    internal ArtemisDevice(IRGBDevice rgbDevice, DeviceProvider deviceProvider, DeviceEntity deviceEntity)
    {
        rgbDevice.EnsureValidDimensions();
        _originalLeds = new List<OriginalLed>(rgbDevice.Select(l => new OriginalLed(l)));
        _originalSize = rgbDevice.Size;

        RgbDevice = rgbDevice;
        Identifier = rgbDevice.GetDeviceIdentifier();
        DeviceEntity = deviceEntity;
        DeviceProvider = deviceProvider;

        LedIds = new ReadOnlyDictionary<LedId, ArtemisLed>(new Dictionary<LedId, ArtemisLed>());
        Leds = new ReadOnlyCollection<ArtemisLed>(new List<ArtemisLed>());
        InputIdentifiers = new List<ArtemisDeviceInputIdentifier>();
        InputMappings = new Dictionary<ArtemisLed, ArtemisLed>();
        Categories = new HashSet<DeviceCategory>();
        LayoutSelection = new LayoutSelection {Type = DefaultLayoutProvider.LayoutType};

        foreach (DeviceInputIdentifierEntity identifierEntity in DeviceEntity.InputIdentifiers)
            InputIdentifiers.Add(new ArtemisDeviceInputIdentifier(identifierEntity.InputProvider, identifierEntity.Identifier));

        RgbDevice.ColorCorrections.Clear();
        RgbDevice.ColorCorrections.Add(new ScaleColorCorrection(this));

        CreateArtemisLeds(false);
        Load();
        ApplyKeyboardLayout();
        CalculateRenderProperties();

        RgbDevice.PropertyChanged += RgbDeviceOnPropertyChanged;
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
    ///     Gets the device type of the ArtemisDevice
    /// </summary>
    public RGBDeviceType DeviceType => RgbDevice.DeviceInfo.DeviceType;

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
    ///     Gets a list of input mappings configured on the device
    /// </summary>
    public Dictionary<ArtemisLed, ArtemisLed> InputMappings { get; }

    /// <summary>
    ///     Gets a list containing the categories of this device
    /// </summary>
    public HashSet<DeviceCategory> Categories { get; }

    public LayoutSelection LayoutSelection { get; }

    /// <summary>
    ///     Gets or sets the X-position of the device
    /// </summary>
    public float X
    {
        get => DeviceEntity.X;
        set
        {
            DeviceEntity.X = value;
            if (RgbDevice.Surface != null)
                ApplyLocation(DeviceEntity.X, DeviceEntity.Y);
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
            if (RgbDevice.Surface != null)
                ApplyLocation(DeviceEntity.X, DeviceEntity.Y);
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
            if (RgbDevice.Surface != null)
                RgbDevice.Rotation = DeviceEntity.Rotation;
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
            if (RgbDevice.Surface != null)
                RgbDevice.Scale = DeviceEntity.Scale;
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
    ///     <para>Note: To enable/disable a device use the methods provided by <see cref="IDeviceService" /></para>
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
    /// <param name="applyInputMapping">
    ///     If <see langword="true" />, LEDs mapped to different LEDs <see cref="InputMappings" />
    ///     are taken into consideration
    /// </param>
    /// <returns>If found, the corresponding <see cref="ArtemisLed" />; otherwise <see langword="null" />.</returns>
    public ArtemisLed? GetLed(Led led, bool applyInputMapping)
    {
        return GetLed(led.Id, applyInputMapping);
    }

    /// <summary>
    ///     Attempts to retrieve the <see cref="ArtemisLed" /> that corresponds the provided RGB.NET <see cref="LedId" />
    /// </summary>
    /// <param name="ledId">The RGB.NET <see cref="LedId" /> to find the corresponding <see cref="ArtemisLed" /> for </param>
    /// <param name="applyInputMapping">
    ///     If <see langword="true" />, LEDs mapped to different LEDs <see cref="InputMappings" />
    ///     are taken into consideration
    /// </param>
    /// <returns>If found, the corresponding <see cref="ArtemisLed" />; otherwise <see langword="null" />.</returns>
    public ArtemisLed? GetLed(LedId ledId, bool applyInputMapping)
    {
        LedIds.TryGetValue(ledId, out ArtemisLed? artemisLed);
        if (artemisLed == null)
            return null;

        if (applyInputMapping && InputMappings.TryGetValue(artemisLed, out ArtemisLed? mappedLed))
            return mappedLed;
        return artemisLed;
    }

    /// <summary>
    ///     Occurs when the underlying RGB.NET device was updated
    /// </summary>
    public event EventHandler? DeviceUpdated;

    /// <summary>
    ///     Applies the default categories for this device to the <see cref="Categories" /> list
    /// </summary>
    public void ApplyDefaultCategories()
    {
        switch (RgbDevice.DeviceInfo.DeviceType)
        {
            case RGBDeviceType.Keyboard:
            case RGBDeviceType.Mouse:
            case RGBDeviceType.Headset:
            case RGBDeviceType.Mousepad:
            case RGBDeviceType.HeadsetStand:
            case RGBDeviceType.Keypad:
                Categories.Add(DeviceCategory.Peripherals);
                break;
            case RGBDeviceType.Mainboard:
            case RGBDeviceType.GraphicsCard:
            case RGBDeviceType.DRAM:
            case RGBDeviceType.Fan:
            case RGBDeviceType.LedStripe:
            case RGBDeviceType.Cooler:
                Categories.Add(DeviceCategory.Case);
                break;
            case RGBDeviceType.Speaker:
                Categories.Add(DeviceCategory.Desk);
                break;
            case RGBDeviceType.Monitor:
                Categories.Add(DeviceCategory.Monitor);
                break;
            case RGBDeviceType.LedMatrix:
                Categories.Add(DeviceCategory.Room);
                break;
        }
    }

    /// <summary>
    ///     Applies the provided layout to the device
    /// </summary>
    /// <param name="layout">The layout to apply</param>
    /// <param name="createMissingLeds">
    ///     A boolean indicating whether to add missing LEDs defined in the layout but missing on
    ///     the device
    /// </param>
    /// <param name="removeExcessiveLeds">
    ///     A boolean indicating whether to remove excess LEDs present in the device but missing
    ///     in the layout
    /// </param>
    public void ApplyLayout(ArtemisLayout? layout, bool createMissingLeds, bool removeExcessiveLeds)
    {
        if (layout != null && layout.IsValid && createMissingLeds && !DeviceProvider.CreateMissingLedsSupported)
            throw new ArtemisCoreException($"Cannot apply layout with {nameof(createMissingLeds)} set to true because the device provider does not support it");
        if (layout != null && layout.IsValid && removeExcessiveLeds && !DeviceProvider.RemoveExcessiveLedsSupported)
            throw new ArtemisCoreException($"Cannot apply layout with {nameof(removeExcessiveLeds)} set to true because the device provider does not support it");

        // Always clear the current layout
        ClearLayout();

        // If a valid layout was supplied, apply the layout to the device
        if (layout != null && layout.IsValid)
        {
            layout.ApplyToDevice(RgbDevice, createMissingLeds, removeExcessiveLeds);
            Layout = layout;
        }
        else
        {
            Layout = null;
        }

        // Recreate Artemis LEDs
        CreateArtemisLeds(true);
        // Calculate render properties with the new layout
        CalculateRenderProperties();
    }

    /// <summary>
    ///     Invokes the <see cref="DeviceUpdated" /> event
    /// </summary>
    protected virtual void OnDeviceUpdated()
    {
        DeviceUpdated?.Invoke(this, EventArgs.Empty);
    }

    private void ClearLayout()
    {
        if (Layout == null)
            return;

        RgbDevice.DeviceInfo.LayoutMetadata = null;
        RgbDevice.Size = _originalSize;
        Layout = null;

        while (RgbDevice.Any())
            RgbDevice.RemoveLed(RgbDevice.First().Id);
        foreach (OriginalLed originalLed in _originalLeds)
            RgbDevice.AddLed(originalLed.Id, originalLed.Location, originalLed.Size, originalLed.CustomData);
    }

    internal void Save()
    {
        // Other properties are computed
        DeviceEntity.Id = Identifier;
        DeviceEntity.DeviceProvider = DeviceProvider.Plugin.Guid.ToString();

        DeviceEntity.InputIdentifiers.Clear();
        foreach (ArtemisDeviceInputIdentifier identifier in InputIdentifiers)
            DeviceEntity.InputIdentifiers.Add(new DeviceInputIdentifierEntity {InputProvider = identifier.InputProvider, Identifier = identifier.Identifier});

        DeviceEntity.InputMappings.Clear();
        foreach ((ArtemisLed? original, ArtemisLed? mapped) in InputMappings)
            DeviceEntity.InputMappings.Add(new InputMappingEntity {OriginalLedId = (int) original.RgbLed.Id, MappedLedId = (int) mapped.RgbLed.Id});

        DeviceEntity.Categories.Clear();
        foreach (DeviceCategory deviceCategory in Categories)
            DeviceEntity.Categories.Add((int) deviceCategory);

        DeviceEntity.LayoutType = LayoutSelection.Type;
        DeviceEntity.LayoutParameter = LayoutSelection.Parameter;
    }

    internal void Load()
    {
        InputIdentifiers.Clear();
        foreach (DeviceInputIdentifierEntity identifierEntity in DeviceEntity.InputIdentifiers)
            InputIdentifiers.Add(new ArtemisDeviceInputIdentifier(identifierEntity.InputProvider, identifierEntity.Identifier));

        Categories.Clear();
        foreach (int deviceEntityCategory in DeviceEntity.Categories)
            Categories.Add((DeviceCategory) deviceEntityCategory);
        if (!Categories.Any())
            ApplyDefaultCategories();

        LayoutSelection.Type = DeviceEntity.LayoutType;
        LayoutSelection.Parameter = DeviceEntity.LayoutParameter;

        LoadInputMappings();
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

        OnDeviceUpdated();
    }

    private void CreateArtemisLeds(bool loadInputMappings)
    {
        Leds = RgbDevice.Select(l => new ArtemisLed(l, this)).ToList().AsReadOnly();
        LedIds = new ReadOnlyDictionary<LedId, ArtemisLed>(Leds.ToDictionary(l => l.RgbLed.Id, l => l));

        if (loadInputMappings)
            LoadInputMappings();
    }

    private void UpdateArtemisLeds()
    {
        foreach (ArtemisLed artemisLed in Leds)
            artemisLed.CalculateRectangles();
    }

    private void LoadInputMappings()
    {
        InputMappings.Clear();
        foreach (InputMappingEntity deviceEntityInputMapping in DeviceEntity.InputMappings)
        {
            ArtemisLed? original = Leds.FirstOrDefault(l => l.RgbLed.Id == (LedId) deviceEntityInputMapping.OriginalLedId);
            ArtemisLed? mapped = Leds.FirstOrDefault(l => l.RgbLed.Id == (LedId) deviceEntityInputMapping.MappedLedId);
            if (original != null && mapped != null)
                InputMappings.Add(original, mapped);
        }
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

    private void RgbDeviceOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(IRGBDevice.Surface) || RgbDevice.Surface == null)
            return;

        RgbDevice.Rotation = DeviceEntity.Rotation;
        RgbDevice.Scale = DeviceEntity.Scale;
        ApplyLocation(DeviceEntity.X, DeviceEntity.Y);
    }

    private void ApplyLocation(float x, float y)
    {
        // Workaround for device rotation not applying
        if (x == 0 && y == 0)
            RgbDevice.Location = new Point(1, 1);
        RgbDevice.Location = new Point(x, y);

        UpdateArtemisLeds();
        CalculateRenderProperties();
    }
}

/// <summary>
///     Represents a device category
/// </summary>
public enum DeviceCategory
{
    /// <summary>
    ///     A device used to light up (part of) the desk
    /// </summary>
    Desk,

    /// <summary>
    ///     A device attached or embedded into the monitor
    /// </summary>
    Monitor,

    /// <summary>
    ///     A device placed or embedded into the case
    /// </summary>
    Case,

    /// <summary>
    ///     A device used to light up (part of) the room
    /// </summary>
    Room,

    /// <summary>
    ///     A peripheral
    /// </summary>
    Peripherals
}