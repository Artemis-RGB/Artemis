using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Exceptions;
using Artemis.UI.Shared;
using ReactiveUI;
using RGB.NET.Core;

namespace Artemis.UI.Screens.Device.Tabs
{
    public class InputMappingsTabViewModel : ActivatableViewModelBase
    {
        private readonly IRgbService _rgbService;
        private readonly IInputService _inputService;
        private readonly ObservableCollection<ArtemisLed> _selectedLeds;
        private ArtemisLed _selectedLed;

        public InputMappingsTabViewModel(ArtemisDevice device, ObservableCollection<ArtemisLed> selectedLeds, IRgbService rgbService, IInputService inputService)
        {
            if (device.DeviceType != RGBDeviceType.Keyboard)
                throw new ArtemisUIException("The input mappings tab only supports keyboards");

            _rgbService = rgbService;
            _inputService = inputService;
            _selectedLeds = selectedLeds;

            Device = device;
            DisplayName = "Input Mappings";
            InputMappings = new ObservableCollection<(ArtemisLed, ArtemisLed)>();
        }

        public ArtemisDevice Device { get; }

        public ArtemisLed SelectedLed
        {
            get => _selectedLed;
            set => this.RaiseAndSetIfChanged(ref _selectedLed, value);
        }

        public ObservableCollection<(ArtemisLed, ArtemisLed)> InputMappings { get; }

        public void DeleteMapping(Tuple<ArtemisLed, ArtemisLed> inputMapping)
        {
            Device.InputMappings.Remove(inputMapping.Item1);
            UpdateInputMappings();
        }

        private void InputServiceOnKeyboardKeyUp(object sender, ArtemisKeyboardKeyEventArgs e)
        {
            if (SelectedLed == null || e.Led == null)
                return;

            // Locate the original LED the same way the InputService did it, but supply false to Device.GetLed
            bool foundLedId = InputKeyUtilities.KeyboardKeyLedIdMap.TryGetValue(e.Key, out LedId ledId);
            if (!foundLedId)
                return;
            ArtemisLed artemisLed = Device.GetLed(ledId, false);
            if (artemisLed == null)
                return;

            // Apply the new LED mapping
            Device.InputMappings[SelectedLed] = artemisLed;
            _rgbService.SaveDevice(Device);
            // ((DeviceDialogViewModel) Parent).SelectedLeds.Clear();

            UpdateInputMappings();
        }

        private void UpdateInputMappings()
        {
            if (InputMappings.Any())
                InputMappings.Clear();

            // InputMappings.AddRange(Device.InputMappings.Select(m => new Tuple<ArtemisLed, ArtemisLed>(m.Key, m.Value)));
        }

        private void SelectedLedsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // SelectedLed = ((DeviceDialogViewModel) Parent).SelectedLeds.FirstOrDefault();
        }
        //
        // #region Overrides of Screen
        //
        // /// <inheritdoc />
        // protected override void OnActivate()
        // {
        //     UpdateInputMappings();
        //     _inputService.KeyboardKeyUp += InputServiceOnKeyboardKeyUp;
        //     ((DeviceDialogViewModel) Parent).SelectedLeds.CollectionChanged += SelectedLedsOnCollectionChanged;
        //
        //     base.OnActivate();
        // }
        //
        // /// <inheritdoc />
        // protected override void OnDeactivate()
        // {
        //     InputMappings.Clear();
        //     _inputService.KeyboardKeyUp -= InputServiceOnKeyboardKeyUp;
        //     ((DeviceDialogViewModel) Parent).SelectedLeds.CollectionChanged -= SelectedLedsOnCollectionChanged;
        //     base.OnDeactivate();
        // }
        //
        // #endregion
    }
}