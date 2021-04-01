using System.Collections.Specialized;
using System.Linq;
using Artemis.Core;
using Stylet;

namespace Artemis.UI.Screens.Settings.Device.Tabs
{
    public class DeviceLedsTabViewModel : Screen
    {
        public DeviceLedsTabViewModel(ArtemisDevice device)
        {
            Device = device;
            DisplayName = "LEDS";
            LedViewModels = new BindableCollection<DeviceLedsTabLedViewModel>();
        }

        public ArtemisDevice Device { get; }
        public BindableCollection<DeviceLedsTabLedViewModel> LedViewModels { get; }

        private void SelectedLedsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateSelectedLeds();
        }

        private void UpdateSelectedLeds()
        {
            foreach (DeviceLedsTabLedViewModel deviceLedsTabLedViewModel in LedViewModels)
                deviceLedsTabLedViewModel.Update();
        }

        #region Overrides of Screen

        /// <inheritdoc />
        protected override void OnInitialActivate()
        {
            BindableCollection<ArtemisLed> selectedLeds = ((DeviceDialogViewModel) Parent).SelectedLeds;
            LedViewModels.Clear();
            LedViewModels.AddRange(Device.Leds.Select(l => new DeviceLedsTabLedViewModel(l, selectedLeds)));
            selectedLeds.CollectionChanged += SelectedLedsOnCollectionChanged;

            base.OnInitialActivate();
        }

        /// <inheritdoc />
        protected override void OnClose()
        {
            ((DeviceDialogViewModel) Parent).SelectedLeds.CollectionChanged -= SelectedLedsOnCollectionChanged;
            base.OnClose();
        }

        #endregion
    }

    public class DeviceLedsTabLedViewModel : PropertyChangedBase
    {
        private readonly BindableCollection<ArtemisLed> _selectedLeds;
        private bool _isSelected;

        public DeviceLedsTabLedViewModel(ArtemisLed artemisLed, BindableCollection<ArtemisLed> selectedLeds)
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
                if (!SetAndNotify(ref _isSelected, value)) return;
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