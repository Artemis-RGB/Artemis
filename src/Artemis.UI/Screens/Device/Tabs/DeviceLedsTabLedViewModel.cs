using System.Collections.ObjectModel;
using Artemis.Core;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.Device;

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
            bool oldValue = _isSelected;
            bool newValue = RaiseAndSetIfChanged(ref _isSelected, value);
            
            if (newValue == oldValue)
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