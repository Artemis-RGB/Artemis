using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.DryIoc.Factories;
using Artemis.UI.Screens.Profiles.ProfileEditor.ProfileTree.Dialogs.AdaptionHints;
using Artemis.UI.Shared;
using DynamicData;
using ReactiveUI;

namespace Artemis.UI.Screens.Profiles.ProfileEditor.ProfileTree.Dialogs;

public class LayerHintsDialogViewModel : DialogViewModelBase<bool>
{
    private readonly IDeviceService _deviceService;
    private readonly ILayerHintVmFactory _vmFactory;

    public LayerHintsDialogViewModel(Layer layer, IDeviceService deviceService, ILayerHintVmFactory vmFactory)
    {
        _deviceService = deviceService;
        _vmFactory = vmFactory;

        Layer = layer;
        AdaptionHints = new ObservableCollection<AdaptionHintViewModelBase>();

        this.WhenActivated(d =>
        {
            Observable.FromEventPattern<LayerAdapterHintEventArgs>(x => layer.Adapter.AdapterHintAdded += x, x => layer.Adapter.AdapterHintAdded -= x)
                .Subscribe(c => AdaptionHints.Add(CreateHintViewModel(c.EventArgs.AdaptionHint)))
                .DisposeWith(d);
            Observable.FromEventPattern<LayerAdapterHintEventArgs>(x => layer.Adapter.AdapterHintRemoved += x, x => layer.Adapter.AdapterHintRemoved -= x)
                .Subscribe(c => AdaptionHints.RemoveMany(AdaptionHints.Where(h => h.AdaptionHint == c.EventArgs.AdaptionHint)))
                .DisposeWith(d);

            AdaptionHints.AddRange(Layer.Adapter.AdaptionHints.Select(CreateHintViewModel));
        });
    }

    public Layer Layer { get; }
    public ObservableCollection<AdaptionHintViewModelBase> AdaptionHints { get; }

    public void Finish()
    {
        Close(true);
    }

    public void AutoDetermineHints()
    {
        Layer.Adapter.DetermineHints(_deviceService.EnabledDevices);
    }

    public void AddCategoryHint()
    {
        Layer.Adapter.Add(new CategoryAdaptionHint());
    }

    public void AddDeviceHint()
    {
        Layer.Adapter.Add(new DeviceAdaptionHint());
    }

    public void AddKeyboardSectionHint()
    {
        Layer.Adapter.Add(new KeyboardSectionAdaptionHint());
    }
    
    public void AddSingleLedHint()
    {
        Layer.Adapter.Add(new SingleLedAdaptionHint());
    }


    public void RemoveAdaptionHint(IAdaptionHint hint)
    {
        Layer.Adapter.Remove(hint);
    }

    private AdaptionHintViewModelBase CreateHintViewModel(IAdaptionHint hint)
    {
        return hint switch
        {
            CategoryAdaptionHint categoryAdaptionHint => _vmFactory.CategoryAdaptionHintViewModel(Layer, categoryAdaptionHint),
            DeviceAdaptionHint deviceAdaptionHint => _vmFactory.DeviceAdaptionHintViewModel(Layer, deviceAdaptionHint),
            KeyboardSectionAdaptionHint keyboardSectionAdaptionHint => _vmFactory.KeyboardSectionAdaptionHintViewModel(Layer, keyboardSectionAdaptionHint),
            SingleLedAdaptionHint singleLedAdaptionHint => _vmFactory.SingleLedAdaptionHintViewModel(Layer, singleLedAdaptionHint),
            _ => throw new ArgumentOutOfRangeException(nameof(hint))
        };
    }
}