using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Exceptions;
using Artemis.UI.Shared;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using RGB.NET.Core;
using Unit = System.Reactive.Unit;

namespace Artemis.UI.Screens.Device.InputMappings;

public partial class InputMappingsTabViewModel : ActivatableViewModelBase
{
    private readonly IInputService _inputService;
    private readonly IDeviceService _deviceService;
    private readonly ObservableCollection<ArtemisLed> _selectedLeds;
    [Notify] private ArtemisLed? _selectedLed;

    public InputMappingsTabViewModel(ArtemisDevice device, ObservableCollection<ArtemisLed> selectedLeds, IInputService inputService, IDeviceService deviceService)
    {
        if (device.DeviceType != RGBDeviceType.Keyboard)
            throw new ArtemisUIException("The input mappings tab only supports keyboards");

        _inputService = inputService;
        _deviceService = deviceService;
        _selectedLeds = selectedLeds;

        Device = device;
        DisplayName = "Input Mappings";
        InputMappings = new ObservableCollection<ArtemisInputMapping>();
        DeleteMapping = ReactiveCommand.Create<ArtemisInputMapping>(ExecuteDeleteMapping);
        
        this.WhenActivated(d =>
        {
            _selectedLeds.CollectionChanged += SelectedLedsOnCollectionChanged;
            _inputService.KeyboardKeyUp += InputServiceOnKeyboardKeyUp;
            UpdateInputMappings();
            Disposable.Create(() =>
            {
                _selectedLeds.CollectionChanged -= SelectedLedsOnCollectionChanged;
                _inputService.KeyboardKeyUp -= InputServiceOnKeyboardKeyUp;
                InputMappings.Clear();
            }).DisposeWith(d);
        });
    }

    public ReactiveCommand<ArtemisInputMapping, Unit> DeleteMapping { get; }
    public ArtemisDevice Device { get; }
    public ObservableCollection<ArtemisInputMapping> InputMappings { get; }

    private void ExecuteDeleteMapping(ArtemisInputMapping inputMapping)
    {
        Device.InputMappings.Remove(inputMapping.Original);
        UpdateInputMappings();
    }

    private void InputServiceOnKeyboardKeyUp(object? sender, ArtemisKeyboardKeyEventArgs e)
    {
        if (SelectedLed == null || e.Led == null)
            return;

        // Locate the original LED the same way the InputService did it, but supply false to Device.GetLed
        bool foundLedId = InputKeyUtilities.TryGetLedIdFromKeyboardKey(e.Key, out LedId ledId);
        if (!foundLedId)
            return;
        ArtemisLed? artemisLed = Device.GetLed(ledId, false);
        if (artemisLed == null)
            return;

        // Apply the new LED mapping
        Device.InputMappings[SelectedLed] = artemisLed;
        _deviceService.SaveDevice(Device);
        _selectedLeds.Clear();

        UpdateInputMappings();
    }

    private void UpdateInputMappings()
    {
        if (InputMappings.Any())
            InputMappings.Clear();

        foreach (ArtemisInputMapping tuple in Device.InputMappings.Select(m => new ArtemisInputMapping(m.Key, m.Value)))
            InputMappings.Add(tuple);
    }

    private void SelectedLedsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        SelectedLed = _selectedLeds.FirstOrDefault();
    }
}

/// <summary>
///     Represents a pair of LEDs, the original and the replacement
/// </summary>
public class ArtemisInputMapping
{
    /// <summary>
    ///     Creates a new instance of the <see cref="ArtemisInputMapping" /> class
    /// </summary>
    public ArtemisInputMapping(ArtemisLed original, ArtemisLed replacement)
    {
        Original = original;
        Replacement = replacement;
    }

    /// <summary>
    ///     The original LED
    /// </summary>
    public ArtemisLed Original { get; set; }
    
    /// <summary>
    ///     The replacement LED
    /// </summary>
    public ArtemisLed Replacement { get; set; }
}