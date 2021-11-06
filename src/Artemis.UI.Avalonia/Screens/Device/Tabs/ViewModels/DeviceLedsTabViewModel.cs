using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Disposables;
using Artemis.Core;
using DynamicData.Binding;
using ReactiveUI;

namespace Artemis.UI.Avalonia.Screens.Device.Tabs.ViewModels
{
    public class DeviceLedsTabViewModel : ActivatableViewModelBase
    {
        private readonly ObservableCollection<ArtemisLed> _selectedLeds;

        public DeviceLedsTabViewModel(ArtemisDevice device, ObservableCollection<ArtemisLed> selectedLeds)
        {
            _selectedLeds = selectedLeds;

            Device = device;
            DisplayName = "LEDs";
            LedViewModels = new ObservableCollection<DeviceLedsTabLedViewModel>(Device.Leds.Select(l => new DeviceLedsTabLedViewModel(l, _selectedLeds)));

            this.WhenActivated(disposables => _selectedLeds.ToObservableChangeSet().Subscribe(_ => UpdateSelectedLeds()).DisposeWith(disposables));
        }

        public ArtemisDevice Device { get; }
        public ObservableCollection<DeviceLedsTabLedViewModel> LedViewModels { get; }

        private void SelectedLedsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateSelectedLeds();
        }

        private void UpdateSelectedLeds()
        {
            foreach (DeviceLedsTabLedViewModel deviceLedsTabLedViewModel in LedViewModels)
                deviceLedsTabLedViewModel.Update();
        }
    }

    public class DeviceLedsTabLedViewModel : ViewModelBase
    {
        private readonly ObservableCollection<ArtemisLed> _selectedLeds;
        private bool _isSelected;

        public DeviceLedsTabLedViewModel(ArtemisLed artemisLed, ObservableCollection<ArtemisLed> selectedLeds)
        {
            _selectedLeds = selectedLeds;
            ArtemisLed = artemisLed;

            Update();
        }

        public ArtemisLed ArtemisLed { get; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (!this.RaiseAndSetIfChanged(ref _isSelected, value))
                    return;
                Apply();
            }
        }

        public void Update()
        {
            IsSelected = _selectedLeds.Contains(ArtemisLed);
        }

        public void Apply()
        {
            if (IsSelected && !_selectedLeds.Contains(ArtemisLed))
                _selectedLeds.Add(ArtemisLed);
            else if (!IsSelected && _selectedLeds.Contains(ArtemisLed))
                _selectedLeds.Remove(ArtemisLed);
        }
    }
}