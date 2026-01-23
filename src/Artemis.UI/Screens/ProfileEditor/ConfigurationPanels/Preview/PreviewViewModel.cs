using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables.Fluent;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.ProfileEditor;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.ConfigurationPanels.Preview;

public class PreviewViewModel : ActivatableViewModelBase
{
    private ObservableAsPropertyHelper<ProfileConfiguration?>? _profileConfiguration;
    
    public PreviewViewModel(IProfileEditorService profileEditorService, IDeviceService deviceService)
    {
        Devices = new ObservableCollection<ArtemisDevice>(deviceService.EnabledDevices.OrderBy(d => d.ZIndex));
        
        this.WhenActivated(d =>
        {
            _profileConfiguration = profileEditorService.ProfileConfiguration.ToProperty(this, vm => vm.ProfileConfiguration).DisposeWith(d);
        });
    }

    public ProfileConfiguration? ProfileConfiguration => _profileConfiguration?.Value;
    public ObservableCollection<ArtemisDevice> Devices { get; }
    
    public void RequestAutoFit()
    {
        AutoFitRequested?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? AutoFitRequested;
}