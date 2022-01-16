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
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.Playback;
using Artemis.UI.Screens.ProfileEditor.ProfileElementProperties.Timeline;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.ProfileEditor;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.ProfileElementProperties;

public class ProfileElementPropertiesViewModel : ActivatableViewModelBase
{
    private readonly Dictionary<LayerPropertyGroup, ProfileElementPropertyGroupViewModel> _cachedViewModels;
    private readonly IProfileEditorService _profileEditorService;
    private readonly ILayerPropertyVmFactory _layerPropertyVmFactory;
    private ObservableAsPropertyHelper<RenderProfileElement?>? _profileElement;
    private ObservableAsPropertyHelper<double>? _pixelsPerSecond;
    private ObservableCollection<ProfileElementPropertyGroupViewModel> _propertyGroupViewModels;

    /// <inheritdoc />
    public ProfileElementPropertiesViewModel(IProfileEditorService profileEditorService, ILayerPropertyVmFactory layerPropertyVmFactory, PlaybackViewModel playbackViewModel, TimelineViewModel timelineViewModel)
    {
        _profileEditorService = profileEditorService;
        _layerPropertyVmFactory = layerPropertyVmFactory;
        _propertyGroupViewModels = new ObservableCollection<ProfileElementPropertyGroupViewModel>();
        _cachedViewModels = new Dictionary<LayerPropertyGroup, ProfileElementPropertyGroupViewModel>();
        PlaybackViewModel = playbackViewModel;
        TimelineViewModel = timelineViewModel;

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
        });
        this.WhenAnyValue(vm => vm.ProfileElement).Subscribe(_ => UpdateGroups());
    }

    public PlaybackViewModel PlaybackViewModel { get; }
    public TimelineViewModel TimelineViewModel { get; }
    public RenderProfileElement? ProfileElement => _profileElement?.Value;
    public Layer? Layer => _profileElement?.Value as Layer;
    public double PixelsPerSecond => _pixelsPerSecond?.Value ?? 0;
    public IObservable<bool> Playing => _profileEditorService.Playing;

    public ObservableCollection<ProfileElementPropertyGroupViewModel> PropertyGroupViewModels
    {
        get => _propertyGroupViewModels;
        set => this.RaiseAndSetIfChanged(ref _propertyGroupViewModels, value);
    }
    
    private void UpdateGroups()
    {
        if (ProfileElement == null)
        {
            PropertyGroupViewModels.Clear();
            return;
        }

        ObservableCollection<ProfileElementPropertyGroupViewModel> viewModels = new();
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
            ProfileElementPropertyGroupViewModel profileElementPropertyGroupViewModel = viewModels[index];
            if (index > PropertyGroupViewModels.Count - 1)
                PropertyGroupViewModels.Add(profileElementPropertyGroupViewModel);
            else if (!ReferenceEquals(PropertyGroupViewModels[index], profileElementPropertyGroupViewModel))
                PropertyGroupViewModels[index] = profileElementPropertyGroupViewModel;
        }

        while (PropertyGroupViewModels.Count > viewModels.Count)
            PropertyGroupViewModels.RemoveAt(PropertyGroupViewModels.Count - 1);
    }

    private ProfileElementPropertyGroupViewModel GetOrCreateViewModel(LayerPropertyGroup layerPropertyGroup, BaseLayerBrush? layerBrush, BaseLayerEffect? layerEffect)
    {
        if (_cachedViewModels.TryGetValue(layerPropertyGroup, out ProfileElementPropertyGroupViewModel? cachedVm))
            return cachedVm;

        ProfileElementPropertyGroupViewModel createdVm;
        if (layerBrush != null)
            createdVm = _layerPropertyVmFactory.ProfileElementPropertyGroupViewModel(layerPropertyGroup, layerBrush);
        else if (layerEffect != null)
            createdVm = _layerPropertyVmFactory.ProfileElementPropertyGroupViewModel(layerPropertyGroup, layerEffect);
        else
            createdVm = _layerPropertyVmFactory.ProfileElementPropertyGroupViewModel(layerPropertyGroup);

        _cachedViewModels[layerPropertyGroup] = createdVm;
        return createdVm;
    }
}