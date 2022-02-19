using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.VisualEditor.Visualizers;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.ProfileEditor;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.VisualEditor;

public class VisualEditorViewModel : ActivatableViewModelBase
{
    private readonly SourceList<IVisualizerViewModel> _visualizers;
    private readonly IProfileEditorVmFactory _vmFactory;
    private ObservableAsPropertyHelper<ProfileConfiguration?>? _profileConfiguration;
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
            _profileConfiguration = profileEditorService.ProfileConfiguration
                .ToProperty(this, vm => vm.ProfileConfiguration)
                .DisposeWith(d);
            profileEditorService.ProfileConfiguration
                .Subscribe(CreateVisualizers)
                .DisposeWith(d);
            profileEditorService.Tools
                .Connect()
                .AutoRefreshOnObservable(t => t.WhenAnyValue(vm => vm.IsSelected)).Filter(t => t.IsSelected).Bind(out ReadOnlyObservableCollection<IToolViewModel> tools)
                .Subscribe()
                .DisposeWith(d);
            Tools = tools;

            this.WhenAnyValue(vm => vm.ProfileConfiguration)
                .Select(p => p?.Profile)
                .Select(p => p != null
                    ? Observable.FromEventPattern<ProfileElementEventArgs>(x => p.DescendentAdded += x, x => p.DescendentAdded -= x)
                    : Observable.Never<EventPattern<ProfileElementEventArgs>>())
                .Switch()
                .Subscribe(AddElement)
                .DisposeWith(d);
            this.WhenAnyValue(vm => vm.ProfileConfiguration)
                .Select(p => p?.Profile)
                .Select(p => p != null
                    ? Observable.FromEventPattern<ProfileElementEventArgs>(x => p.DescendentRemoved += x, x => p.DescendentRemoved -= x)
                    : Observable.Never<EventPattern<ProfileElementEventArgs>>())
                .Switch()
                .Subscribe(RemoveElement)
                .DisposeWith(d);
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

    private void RemoveElement(EventPattern<ProfileElementEventArgs> eventPattern)
    {
        List<IVisualizerViewModel> visualizers = Visualizers.Where(v => v.ProfileElement == eventPattern.EventArgs.ProfileElement).ToList();
        if (visualizers.Any())
            _visualizers.RemoveMany(visualizers);
    }

    private void AddElement(EventPattern<ProfileElementEventArgs> eventPattern)
    {
        if (eventPattern.EventArgs.ProfileElement is Layer layer)
            _visualizers.Edit(list => CreateVisualizer(list, layer));
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