using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.Core.LayerBrushes;
using Artemis.Core.LayerEffects;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.Playback;
using Artemis.UI.Screens.ProfileEditor.Properties.Timeline;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.ProfileEditor;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.Properties;

public class PropertiesViewModel : ActivatableViewModelBase
{
    private readonly Dictionary<LayerPropertyGroup, PropertyGroupViewModel> _cachedViewModels;
    private readonly ILayerPropertyVmFactory _layerPropertyVmFactory;
    private readonly IProfileEditorService _profileEditorService;
    private readonly ISettingsService _settingsService;
    private ObservableAsPropertyHelper<int>? _pixelsPerSecond;
    private ObservableAsPropertyHelper<RenderProfileElement?>? _profileElement;

    /// <inheritdoc />
    public PropertiesViewModel(IProfileEditorService profileEditorService, ISettingsService settingsService, ILayerPropertyVmFactory layerPropertyVmFactory, PlaybackViewModel playbackViewModel)
    {
        _profileEditorService = profileEditorService;
        _settingsService = settingsService;
        _layerPropertyVmFactory = layerPropertyVmFactory;
        _cachedViewModels = new Dictionary<LayerPropertyGroup, PropertyGroupViewModel>();
        PropertyGroupViewModels = new ObservableCollection<PropertyGroupViewModel>();
        PlaybackViewModel = playbackViewModel;
        TimelineViewModel = layerPropertyVmFactory.TimelineViewModel(PropertyGroupViewModels);

        // Subscribe to events of the latest selected profile element - borrowed from https://stackoverflow.com/a/63950940
        this.WhenAnyValue(vm => vm.ProfileElement)
            .Select(p => p is Layer l
                ? Observable.FromEventPattern(x => l.LayerBrushUpdated += x, x => l.LayerBrushUpdated -= x)
                : Observable.Never<EventPattern<object>>())
            .Switch()
            .Subscribe(_ => UpdateGroups());
        this.WhenAnyValue(vm => vm.ProfileElement)
            .Select(p => p != null
                ? Observable.FromEventPattern(x => p.LayerEffectsUpdated += x, x => p.LayerEffectsUpdated -= x)
                : Observable.Never<EventPattern<object>>())
            .Switch()
            .Subscribe(_ => UpdateGroups());
        // React to service profile element changes as long as the VM is active

        this.WhenActivated(d =>
        {
            _profileElement = profileEditorService.ProfileElement.ToProperty(this, vm => vm.ProfileElement).DisposeWith(d);
            _pixelsPerSecond = profileEditorService.PixelsPerSecond.ToProperty(this, vm => vm.PixelsPerSecond).DisposeWith(d);
            Disposable.Create(() => _settingsService.SaveAllSettings()).DisposeWith(d);
        });
        this.WhenAnyValue(vm => vm.ProfileElement).Subscribe(_ => UpdateGroups());
    }

    public ObservableCollection<PropertyGroupViewModel> PropertyGroupViewModels { get; }
    public PlaybackViewModel PlaybackViewModel { get; }
    public TimelineViewModel TimelineViewModel { get; }

    public RenderProfileElement? ProfileElement => _profileElement?.Value;
    public Layer? Layer => _profileElement?.Value as Layer;

    public int PixelsPerSecond => _pixelsPerSecond?.Value ?? 0;
    public IObservable<bool> Playing => _profileEditorService.Playing;
    public PluginSetting<double> PropertiesTreeWidth => _settingsService.GetSetting("ProfileEditor.PropertiesTreeWidth", 500.0);
    
    private void UpdateGroups()
    {
        if (ProfileElement == null)
        {
            PropertyGroupViewModels.Clear();
            return;
        }

        ObservableCollection<PropertyGroupViewModel> viewModels = new();
        if (Layer != null)
        {
            // Add base VMs
            viewModels.Add(GetOrCreateViewModel(Layer.General, null, null));
            viewModels.Add(GetOrCreateViewModel(Layer.Transform, null, null));

            // Add brush VM if the brush has properties
            if (Layer.LayerBrush?.BaseProperties != null)
                viewModels.Add(GetOrCreateViewModel(Layer.LayerBrush.BaseProperties, Layer.LayerBrush, null));
        }

        // Add effect VMs
        foreach (BaseLayerEffect layerEffect in ProfileElement.LayerEffects.OrderBy(e => e.Order))
            if (layerEffect.BaseProperties != null)
                viewModels.Add(GetOrCreateViewModel(layerEffect.BaseProperties, null, layerEffect));

        // Map the most recent collection of VMs to the current list of VMs, making as little changes to the collection as possible
        for (int index = 0; index < viewModels.Count; index++)
        {
            PropertyGroupViewModel propertyGroupViewModel = viewModels[index];
            if (index > PropertyGroupViewModels.Count - 1)
                PropertyGroupViewModels.Add(propertyGroupViewModel);
            else if (!ReferenceEquals(PropertyGroupViewModels[index], propertyGroupViewModel))
                PropertyGroupViewModels[index] = propertyGroupViewModel;
        }

        while (PropertyGroupViewModels.Count > viewModels.Count)
            PropertyGroupViewModels.RemoveAt(PropertyGroupViewModels.Count - 1);
    }

    private PropertyGroupViewModel GetOrCreateViewModel(LayerPropertyGroup layerPropertyGroup, BaseLayerBrush? layerBrush, BaseLayerEffect? layerEffect)
    {
        if (_cachedViewModels.TryGetValue(layerPropertyGroup, out PropertyGroupViewModel? cachedVm))
            return cachedVm;

        PropertyGroupViewModel createdVm;
        if (layerBrush != null)
            createdVm = _layerPropertyVmFactory.PropertyGroupViewModel(layerPropertyGroup, layerBrush);
        else if (layerEffect != null)
            createdVm = _layerPropertyVmFactory.PropertyGroupViewModel(layerPropertyGroup, layerEffect);
        else
            createdVm = _layerPropertyVmFactory.PropertyGroupViewModel(layerPropertyGroup);

        _cachedViewModels[layerPropertyGroup] = createdVm;
        return createdVm;
    }
}