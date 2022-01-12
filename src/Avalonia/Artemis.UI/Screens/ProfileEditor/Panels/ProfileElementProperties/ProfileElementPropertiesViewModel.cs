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
    private readonly Dictionary<RenderProfileElement, List<ProfileElementPropertyGroupViewModel>> _profileElementGroups;
    private ObservableCollection<ProfileElementPropertyGroupViewModel> _propertyGroupViewModels;

    /// <inheritdoc />
    public ProfileElementPropertiesViewModel(IProfileEditorService profileEditorService, ILayerPropertyVmFactory layerPropertyVmFactory)
    {
        _profileEditorService = profileEditorService;
        _layerPropertyVmFactory = layerPropertyVmFactory;
        PropertyGroupViewModels = new ObservableCollection<ProfileElementPropertyGroupViewModel>();
        _profileElementGroups = new Dictionary<RenderProfileElement, List<ProfileElementPropertyGroupViewModel>>();

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

        this.WhenActivated(d => _profileElement = _profileEditorService.ProfileElement.ToProperty(this, vm => vm.ProfileElement).DisposeWith(d));
        this.WhenAnyValue(vm => vm.ProfileElement).Subscribe(_ => UpdateGroups());
    }

    public RenderProfileElement? ProfileElement => _profileElement?.Value;
    public Layer? Layer => _profileElement?.Value as Layer;

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

        if (!_profileElementGroups.TryGetValue(ProfileElement, out List<ProfileElementPropertyGroupViewModel>? viewModels))
        {
            viewModels = new List<ProfileElementPropertyGroupViewModel>();
            _profileElementGroups[ProfileElement] = viewModels;
        }

        List<LayerPropertyGroup> groups = new();

        if (Layer != null)
        {
            // Add default layer groups
            groups.Add(Layer.General);
            groups.Add(Layer.Transform);
            // Add brush group
            if (Layer.LayerBrush?.BaseProperties != null)
                groups.Add(Layer.LayerBrush.BaseProperties);
        }

        // Add effect groups
        foreach (BaseLayerEffect layerEffect in ProfileElement.LayerEffects)
        {
            if (layerEffect.BaseProperties != null)
                groups.Add(layerEffect.BaseProperties);
        }

        // Remove redundant VMs
        viewModels.RemoveAll(vm => !groups.Contains(vm.LayerPropertyGroup));

        // Create VMs for missing groups
        foreach (LayerPropertyGroup group in groups)
        {
            if (viewModels.All(vm => vm.LayerPropertyGroup != group))
                viewModels.Add(_layerPropertyVmFactory.ProfileElementPropertyGroupViewModel(group));
        }

        // Get all non-effect properties
        List<ProfileElementPropertyGroupViewModel> nonEffectProperties = viewModels
            .Where(l => l.TreeGroupViewModel.GroupType != LayerPropertyGroupType.LayerEffectRoot)
            .ToList();
        // Order the effects
        List<ProfileElementPropertyGroupViewModel> effectProperties = viewModels
            .Where(l => l.TreeGroupViewModel.GroupType == LayerPropertyGroupType.LayerEffectRoot && l.LayerPropertyGroup.LayerEffect != null)
            .OrderBy(l => l.LayerPropertyGroup.LayerEffect?.Order)
            .ToList();

        ObservableCollection<ProfileElementPropertyGroupViewModel> propertyGroupViewModels = new();
        foreach (ProfileElementPropertyGroupViewModel viewModel in nonEffectProperties)
            propertyGroupViewModels.Add(viewModel);
        foreach (ProfileElementPropertyGroupViewModel viewModel in effectProperties)
            propertyGroupViewModels.Add(viewModel);

        PropertyGroupViewModels = propertyGroupViewModels;
    }
}