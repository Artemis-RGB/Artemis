using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.LayerBrushes;
using Artemis.Core.LayerEffects;
using Artemis.Core.Services;
using Artemis.UI.DryIoc.Factories;
using Artemis.UI.Screens.ProfileEditor.Playback;
using Artemis.UI.Screens.ProfileEditor.Properties.DataBinding;
using Artemis.UI.Screens.ProfileEditor.Properties.Dialogs;
using Artemis.UI.Screens.ProfileEditor.Properties.Timeline;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.ProfileEditor;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.Properties;

public class PropertiesViewModel : ActivatableViewModelBase
{
    private readonly Dictionary<LayerPropertyGroup, PropertyGroupViewModel> _cachedPropertyViewModels;
    private readonly IDataBindingVmFactory _dataBindingVmFactory;
    private readonly ILayerEffectService _layerEffectService;
    private readonly ILayerPropertyVmFactory _layerPropertyVmFactory;
    private readonly IProfileEditorService _profileEditorService;
    private readonly ISettingsService _settingsService;
    private readonly IWindowService _windowService;
    private DataBindingViewModel? _backgroundDataBindingViewModel;
    private DataBindingViewModel? _dataBindingViewModel;
    private ObservableAsPropertyHelper<ILayerProperty?>? _layerProperty;
    private ObservableAsPropertyHelper<int>? _pixelsPerSecond;
    private ObservableAsPropertyHelper<RenderProfileElement?>? _profileElement;
    private ObservableAsPropertyHelper<bool>? _suspendedEditing;

    /// <inheritdoc />
    public PropertiesViewModel(IProfileEditorService profileEditorService,
        ISettingsService settingsService,
        ILayerPropertyVmFactory layerPropertyVmFactory,
        IDataBindingVmFactory dataBindingVmFactory,
        IWindowService windowService,
        ILayerEffectService layerEffectService,
        PlaybackViewModel playbackViewModel)
    {
        _profileEditorService = profileEditorService;
        _settingsService = settingsService;
        _layerPropertyVmFactory = layerPropertyVmFactory;
        _dataBindingVmFactory = dataBindingVmFactory;
        _windowService = windowService;
        _layerEffectService = layerEffectService;
        _cachedPropertyViewModels = new Dictionary<LayerPropertyGroup, PropertyGroupViewModel>();

        PropertyGroupViewModels = new ObservableCollection<PropertyGroupViewModel>();
        PlaybackViewModel = playbackViewModel;
        TimelineViewModel = layerPropertyVmFactory.TimelineViewModel(PropertyGroupViewModels);
        AddEffect = ReactiveCommand.CreateFromTask(ExecuteAddEffect);
        // React to service profile element changes as long as the VM is active
        this.WhenActivated(d =>
        {
            _profileElement = profileEditorService.ProfileElement.ToProperty(this, vm => vm.ProfileElement).DisposeWith(d);
            _pixelsPerSecond = profileEditorService.PixelsPerSecond.ToProperty(this, vm => vm.PixelsPerSecond).DisposeWith(d);
            _layerProperty = profileEditorService.LayerProperty.ToProperty(this, vm => vm.LayerProperty).DisposeWith(d);
            _suspendedEditing = profileEditorService.SuspendedEditing.ToProperty(this, vm => vm.SuspendedEditing).DisposeWith(d);
            Disposable.Create(() =>
            {
                _settingsService.SaveAllSettings();
                foreach ((LayerPropertyGroup _, PropertyGroupViewModel value) in _cachedPropertyViewModels)
                    value.Dispose();
                _cachedPropertyViewModels.Clear();
            }).DisposeWith(d);
        });

        // Subscribe to events of the latest selected profile element - borrowed from https://stackoverflow.com/a/63950940
        this.WhenActivated(d =>
        {
            this.WhenAnyValue(vm => vm.ProfileElement)
                .Select(p => p is Layer l
                    ? Observable.FromEventPattern(x => l.LayerBrushUpdated += x, x => l.LayerBrushUpdated -= x)
                    : Observable.Never<EventPattern<object>>())
                .Switch()
                .ObserveOn(Shared.UI.BackgroundScheduler)
                .Subscribe(_ => UpdatePropertyGroups())
                .DisposeWith(d);
            this.WhenAnyValue(vm => vm.ProfileElement)
                .Select(p => p != null
                    ? Observable.FromEventPattern(x => p.LayerEffectsUpdated += x, x => p.LayerEffectsUpdated -= x)
                    : Observable.Never<EventPattern<object>>())
                .Switch()
                .ObserveOn(Shared.UI.BackgroundScheduler)
                .Subscribe(_ => UpdatePropertyGroups())
                .DisposeWith(d);
        });
        this.WhenAnyValue(vm => vm.ProfileElement).ObserveOn(Shared.UI.BackgroundScheduler).Subscribe(_ => UpdatePropertyGroups());
        this.WhenAnyValue(vm => vm.LayerProperty).Subscribe(_ => UpdateTimelineViewModel());
    }

    public ObservableCollection<PropertyGroupViewModel> PropertyGroupViewModels { get; }
    public PlaybackViewModel PlaybackViewModel { get; }
    public TimelineViewModel TimelineViewModel { get; }
    public ReactiveCommand<Unit, Unit> AddEffect { get; }

    public DataBindingViewModel? DataBindingViewModel
    {
        get => _dataBindingViewModel;
        set => RaiseAndSetIfChanged(ref _dataBindingViewModel, value);
    }

    public RenderProfileElement? ProfileElement => _profileElement?.Value;
    public ILayerProperty? LayerProperty => _layerProperty?.Value;
    public bool SuspendedEditing => _suspendedEditing?.Value ?? false;

    public int PixelsPerSecond => _pixelsPerSecond?.Value ?? 0;
    public IObservable<bool> Playing => _profileEditorService.Playing;
    public PluginSetting<double> PropertiesTreeWidth => _settingsService.GetSetting("ProfileEditor.PropertiesTreeWidth", 500.0);

    private async Task ExecuteAddEffect()
    {
        if (ProfileElement == null)
            return;

        await _windowService.CreateContentDialog()
            .WithTitle("Add layer effect")
            .WithViewModel(out AddEffectViewModel _, ProfileElement)
            .WithCloseButtonText("Cancel")
            .ShowAsync();
    }

    private void UpdatePropertyGroups()
    {
        RenderProfileElement? profileElement = ProfileElement;
        if (profileElement == null)
        {
            PropertyGroupViewModels.Clear();
            return;
        }

        ObservableCollection<PropertyGroupViewModel> viewModels = new();
        if (profileElement is Layer layer)
        {
            // Add base VMs
            viewModels.Add(GetOrCreatePropertyViewModel(layer.General, null, null));
            viewModels.Add(GetOrCreatePropertyViewModel(layer.Transform, null, null));

            // Add brush VM if the brush has properties
            if (layer.LayerBrush?.BaseProperties != null)
                viewModels.Add(GetOrCreatePropertyViewModel(layer.LayerBrush.BaseProperties, layer.LayerBrush, null));
        }

        // Add effect VMs
        foreach (BaseLayerEffect layerEffect in profileElement.LayerEffects.OrderBy(e => e.Order))
        {
            if (layerEffect.BaseProperties != null)
                viewModels.Add(GetOrCreatePropertyViewModel(layerEffect.BaseProperties, null, layerEffect));
        }

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

    private PropertyGroupViewModel GetOrCreatePropertyViewModel(LayerPropertyGroup layerPropertyGroup, BaseLayerBrush? layerBrush, BaseLayerEffect? layerEffect)
    {
        if (_cachedPropertyViewModels.TryGetValue(layerPropertyGroup, out PropertyGroupViewModel? cachedVm))
            return cachedVm;

        PropertyGroupViewModel createdVm;
        if (layerBrush != null)
            createdVm = _layerPropertyVmFactory.PropertyGroupViewModel(layerPropertyGroup, layerBrush);
        else if (layerEffect != null)
            createdVm = _layerPropertyVmFactory.PropertyGroupViewModel(layerPropertyGroup, layerEffect);
        else
            createdVm = _layerPropertyVmFactory.PropertyGroupViewModel(layerPropertyGroup);

        _cachedPropertyViewModels[layerPropertyGroup] = createdVm;
        return createdVm;
    }

    private void UpdateTimelineViewModel()
    {
        if (LayerProperty == null)
        {
            DataBindingViewModel = null;
        }
        else
        {
            _backgroundDataBindingViewModel ??= _dataBindingVmFactory.DataBindingViewModel();
            DataBindingViewModel = _backgroundDataBindingViewModel;
        }
    }
}