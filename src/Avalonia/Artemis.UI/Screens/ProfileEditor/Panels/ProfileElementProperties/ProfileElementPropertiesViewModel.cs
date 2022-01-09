using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.Core.LayerEffects;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.ProfileElementProperties.Tree;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.ProfileEditor;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.ProfileElementProperties;

public class ProfileElementPropertiesViewModel : ActivatableViewModelBase
{
    private readonly ILayerPropertyVmFactory _layerPropertyVmFactory;
    private readonly IProfileEditorService _profileEditorService;
    private ProfileElementPropertyGroupViewModel? _brushPropertyGroup;
    private ObservableAsPropertyHelper<RenderProfileElement?>? _profileElement;

    /// <inheritdoc />
    public ProfileElementPropertiesViewModel(IProfileEditorService profileEditorService, ILayerPropertyVmFactory layerPropertyVmFactory)
    {
        _profileEditorService = profileEditorService;
        _layerPropertyVmFactory = layerPropertyVmFactory;
        PropertyGroupViewModels = new ObservableCollection<ProfileElementPropertyGroupViewModel>();

        // Subscribe to events of the latest selected profile element - borrowed from https://stackoverflow.com/a/63950940
        this.WhenAnyValue(x => x.ProfileElement)
            .Select(p => p is Layer l
                ? Observable.FromEventPattern(x => l.LayerBrushUpdated += x, x => l.LayerBrushUpdated -= x)
                : Observable.Never<EventPattern<object>>())
            .Switch()
            .Subscribe(_ => ApplyEffects());
        this.WhenAnyValue(x => x.ProfileElement)
            .Select(p => p != null
                ? Observable.FromEventPattern(x => p.LayerEffectsUpdated += x, x => p.LayerEffectsUpdated -= x)
                : Observable.Never<EventPattern<object>>())
            .Switch()
            .Subscribe(_ => ApplyLayerBrush());

        // React to service profile element changes as long as the VM is active
        this.WhenActivated(d =>
        {
            _profileElement = _profileEditorService.ProfileElement.ToProperty(this, vm => vm.ProfileElement).DisposeWith(d);
            _profileEditorService.ProfileElement.Subscribe(p => PopulateProperties(p)).DisposeWith(d);
        });
    }

    public RenderProfileElement? ProfileElement => _profileElement?.Value;
    public Layer? Layer => _profileElement?.Value as Layer;
    public ObservableCollection<ProfileElementPropertyGroupViewModel> PropertyGroupViewModels { get; }

    private void PopulateProperties(RenderProfileElement? renderProfileElement)
    {
        PropertyGroupViewModels.Clear();
        _brushPropertyGroup = null;

        if (ProfileElement == null)
            return;

        // Add layer root groups
        if (Layer != null)
        {
            PropertyGroupViewModels.Add(_layerPropertyVmFactory.ProfileElementPropertyGroupViewModel(Layer.General));
            PropertyGroupViewModels.Add(_layerPropertyVmFactory.ProfileElementPropertyGroupViewModel(Layer.Transform));
            ApplyLayerBrush(false);
        }

        ApplyEffects();
    }

    private void ApplyLayerBrush(bool sortProperties = true)
    {
        if (Layer == null)
            return;

        bool hideRenderRelatedProperties = Layer.LayerBrush != null && Layer.LayerBrush.SupportsTransformation;

        Layer.General.ShapeType.IsHidden = hideRenderRelatedProperties;
        Layer.General.BlendMode.IsHidden = hideRenderRelatedProperties;
        Layer.Transform.IsHidden = hideRenderRelatedProperties;

        if (_brushPropertyGroup != null)
        {
            PropertyGroupViewModels.Remove(_brushPropertyGroup);
            _brushPropertyGroup = null;
        }

        if (Layer.LayerBrush?.BaseProperties != null)
        {
            _brushPropertyGroup = _layerPropertyVmFactory.ProfileElementPropertyGroupViewModel(Layer.LayerBrush.BaseProperties);
            PropertyGroupViewModels.Add(_brushPropertyGroup);
        }

        if (sortProperties)
            SortProperties();
    }

    private void ApplyEffects(bool sortProperties = true)
    {
        if (ProfileElement == null)
            return;

        // Remove VMs of effects no longer applied on the layer
        List<ProfileElementPropertyGroupViewModel> toRemove = PropertyGroupViewModels
            .Where(l => l.LayerPropertyGroup.LayerEffect != null && !ProfileElement.LayerEffects.Contains(l.LayerPropertyGroup.LayerEffect))
            .ToList();
        foreach (ProfileElementPropertyGroupViewModel profileElementPropertyGroupViewModel in toRemove)
            PropertyGroupViewModels.Remove(profileElementPropertyGroupViewModel);

        foreach (BaseLayerEffect layerEffect in ProfileElement.LayerEffects)
        {
            if (PropertyGroupViewModels.Any(l => l.LayerPropertyGroup.LayerEffect == layerEffect) || layerEffect.BaseProperties == null)
                continue;

            PropertyGroupViewModels.Add(_layerPropertyVmFactory.ProfileElementPropertyGroupViewModel(layerEffect.BaseProperties));
        }

        if (sortProperties)
            SortProperties();
    }

    private void SortProperties()
    {
        // Get all non-effect properties
        List<ProfileElementPropertyGroupViewModel> nonEffectProperties = PropertyGroupViewModels
            .Where(l => l.TreeGroupViewModel.GroupType != LayerPropertyGroupType.LayerEffectRoot)
            .ToList();
        // Order the effects
        List<ProfileElementPropertyGroupViewModel> effectProperties = PropertyGroupViewModels
            .Where(l => l.TreeGroupViewModel.GroupType == LayerPropertyGroupType.LayerEffectRoot && l.LayerPropertyGroup.LayerEffect != null)
            .OrderBy(l => l.LayerPropertyGroup.LayerEffect?.Order)
            .ToList();

        // Put the non-effect properties in front
        for (int index = 0; index < nonEffectProperties.Count; index++)
        {
            ProfileElementPropertyGroupViewModel layerPropertyGroupViewModel = nonEffectProperties[index];
            if (PropertyGroupViewModels.IndexOf(layerPropertyGroupViewModel) != index)
                PropertyGroupViewModels.Move(PropertyGroupViewModels.IndexOf(layerPropertyGroupViewModel), index);
        }

        // Put the effect properties after, sorted by their order
        for (int index = 0; index < effectProperties.Count; index++)
        {
            ProfileElementPropertyGroupViewModel layerPropertyGroupViewModel = effectProperties[index];
            if (PropertyGroupViewModels.IndexOf(layerPropertyGroupViewModel) != index + nonEffectProperties.Count)
                PropertyGroupViewModels.Move(PropertyGroupViewModels.IndexOf(layerPropertyGroupViewModel), index + nonEffectProperties.Count);
        }
    }
}