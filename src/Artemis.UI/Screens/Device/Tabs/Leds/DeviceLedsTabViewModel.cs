using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Disposables.Fluent;
using Artemis.Core;
using Artemis.UI.Shared;
using DynamicData.Binding;
using ReactiveUI;

namespace Artemis.UI.Screens.Device.Leds;

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