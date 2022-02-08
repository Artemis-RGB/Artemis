using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.VisualEditor.Tools;
using Artemis.UI.Screens.ProfileEditor.VisualEditor.Visualizers;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.ProfileEditor;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.VisualEditor;

public class VisualEditorViewModel : ActivatableViewModelBase
{
    private readonly IProfileEditorVmFactory _vmFactory;
    private ObservableAsPropertyHelper<ProfileConfiguration?>? _profileConfiguration;
    private readonly SourceList<IVisualizerViewModel> _visualizers;
    private ReadOnlyObservableCollection<IToolViewModel> _tools;

    public VisualEditorViewModel(IProfileEditorService profileEditorService, IRgbService rgbService, IProfileEditorVmFactory vmFactory)
    {
        _vmFactory = vmFactory;
        _visualizers = new SourceList<IVisualizerViewModel>();
        _visualizers.Connect()
            .Sort(SortExpressionComparer<IVisualizerViewModel>.Ascending(vm => vm.Order))
            .Bind(out ReadOnlyObservableCollection<IVisualizerViewModel> visualizers)
            .Subscribe();

        Devices = new ObservableCollection<ArtemisDevice>(rgbService.EnabledDevices);
        Visualizers = visualizers;

        this.WhenActivated(d =>
        {
            _profileConfiguration = profileEditorService.ProfileConfiguration.ToProperty(this, vm => vm.ProfileConfiguration).DisposeWith(d);
            profileEditorService.ProfileConfiguration.Subscribe(CreateVisualizers).DisposeWith(d);
            profileEditorService.Tools.Connect().AutoRefreshOnObservable(t => t.WhenAnyValue(vm => vm.IsSelected)).Filter(t => t.IsSelected).Bind(out ReadOnlyObservableCollection<IToolViewModel> tools).Subscribe().DisposeWith(d);
            Tools = tools;
        });
    }

    public ProfileConfiguration? ProfileConfiguration => _profileConfiguration?.Value;

    public ObservableCollection<ArtemisDevice> Devices { get; }
    public ReadOnlyObservableCollection<IVisualizerViewModel> Visualizers { get; }

    public ReadOnlyObservableCollection<IToolViewModel> Tools
    {
        get => _tools;
        set => RaiseAndSetIfChanged(ref _tools, value);
    }

    private void CreateVisualizers(ProfileConfiguration? profileConfiguration)
    {
        _visualizers.Edit(list =>
        {
            list.Clear();
            if (profileConfiguration?.Profile == null)
                return;
            foreach (Layer layer in profileConfiguration.Profile.GetAllLayers())
                CreateVisualizer(list, layer);
        });
    }

    private void CreateVisualizer(ICollection<IVisualizerViewModel> visualizerViewModels, Layer layer)
    {
        visualizerViewModels.Add(_vmFactory.LayerShapeVisualizerViewModel(layer));
        visualizerViewModels.Add(_vmFactory.LayerVisualizerViewModel(layer));
    }
}