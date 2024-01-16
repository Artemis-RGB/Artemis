using System.Collections.ObjectModel;
using Artemis.Core;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.Device.Leds;

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
            RaiseAndSetIfChanged(ref _isSelected, value);
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