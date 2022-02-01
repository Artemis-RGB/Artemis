using System;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.VisualEditor.Tools;
using Artemis.UI.Screens.ProfileEditor.VisualEditor.Visualizers;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.ProfileEditor;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.VisualEditor;

public class VisualEditorViewModel : ActivatableViewModelBase
{
    private readonly IProfileEditorVmFactory _vmFactory;
    private ObservableAsPropertyHelper<ProfileConfiguration?>? _profileConfiguration;

    public VisualEditorViewModel(IProfileEditorService profileEditorService, IRgbService rgbService, IProfileEditorVmFactory vmFactory)
    {
        _vmFactory = vmFactory;
        Devices = new ObservableCollection<ArtemisDevice>(rgbService.EnabledDevices);
        Visualizers = new ObservableCollection<IVisualizerViewModel>();
        Tools = new ObservableCollection<IToolViewModel>();

        this.WhenActivated(d =>
        {
            _profileConfiguration = profileEditorService.ProfileConfiguration.ToProperty(this, vm => vm.ProfileConfiguration).DisposeWith(d);
            profileEditorService.ProfileConfiguration.Subscribe(CreateVisualizers).DisposeWith(d);
        });
    }

    public ProfileConfiguration? ProfileConfiguration => _profileConfiguration?.Value;

    public ObservableCollection<ArtemisDevice> Devices { get; }
    public ObservableCollection<IVisualizerViewModel> Visualizers { get; set; }
    public ObservableCollection<IToolViewModel> Tools { get; set; }

    private void CreateVisualizers(ProfileConfiguration? profileConfiguration)
    {
        Visualizers.Clear();
        if (profileConfiguration?.Profile == null)
            return;

        foreach (Layer layer in profileConfiguration.Profile.GetAllLayers())
            CreateVisualizer(layer);
    }

    private void CreateVisualizer(Layer layer)
    {
        Visualizers.Add(_vmFactory.LayerVisualizerViewModel(layer));
    }
}