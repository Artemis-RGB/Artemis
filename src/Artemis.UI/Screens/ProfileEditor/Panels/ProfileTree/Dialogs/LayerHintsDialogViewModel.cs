using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.ProfileTree.Dialogs.AdaptionHints;
using Artemis.UI.Shared;
using Avalonia.Controls.Mixins;
using DynamicData;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree.Dialogs;

public class LayerHintsDialogViewModel : DialogViewModelBase<bool>
{
    private readonly IRgbService _rgbService;
    private readonly ILayerHintVmFactory _vmFactory;

    public LayerHintsDialogViewModel(Layer layer, IRgbService rgbService, ILayerHintVmFactory vmFactory)
    {
        _rgbService = rgbService;
        _vmFactory = vmFactory;

        Layer = layer;
        AdaptionHints = new ObservableCollection<AdaptionHintViewModelBase>();

        this.WhenActivated(d =>
        {
            Observable.FromEventPattern<LayerAdapterHintEventArgs>(x => layer.Adapter.AdapterHintAdded += x, x => layer.Adapter.AdapterHintAdded -= x)
                .Subscribe(c => AdaptionHints.Add(CreateHintViewModel(c.EventArgs.AdaptionHint)))
                .DisposeWith(d);
            Observable.FromEventPattern<LayerAdapterHintEventArgs>(x => layer.Adapter.AdapterHintRemoved += x, x => layer.Adapter.AdapterHintRemoved -= x)
                .Subscribe(c => AdaptionHints.Remove(AdaptionHints.FirstOrDefault(h => h.AdaptionHint == c.EventArgs.AdaptionHint)!))
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
        Layer.Adapter.DetermineHints(_rgbService.EnabledDevices);
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

    public void RemoveAdaptionHint(IAdaptionHint hint)
    {
        Layer.Adapter.Remove(hint);
    }

    private AdaptionHintViewModelBase CreateHintViewModel(IAdaptionHint hint)
    {
        return hint switch
        {
            CategoryAdaptionHint categoryAdaptionHint => _vmFactory.CategoryAdaptionHintViewModel(categoryAdaptionHint),
            DeviceAdaptionHint deviceAdaptionHint => _vmFactory.DeviceAdaptionHintViewModel(deviceAdaptionHint),
            KeyboardSectionAdaptionHint keyboardSectionAdaptionHint => _vmFactory.KeyboardSectionAdaptionHintViewModel(keyboardSectionAdaptionHint),
            _ => throw new ArgumentOutOfRangeException(nameof(hint))
        };
    }
}