using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using Serilog;

namespace Artemis.UI.Screens.Workshop.LayoutFinder;

public partial class LayoutFinderViewModel : ActivatableViewModelBase
{
    private readonly ILogger _logger;
    [Notify] private ReadOnlyObservableCollection<LayoutFinderDeviceViewModel> _deviceViewModels;

    public LayoutFinderViewModel(ILogger logger, IDeviceService deviceService, Func<ArtemisDevice, LayoutFinderDeviceViewModel> getDeviceViewModel)
    {
        _logger = logger;
        SearchAll = ReactiveCommand.CreateFromTask(ExecuteSearchAll);
        DeviceViewModels = new ReadOnlyObservableCollection<LayoutFinderDeviceViewModel>([]);
        
        this.WhenActivated((CompositeDisposable _) =>
        {
            IEnumerable<LayoutFinderDeviceViewModel> deviceGroups = deviceService.EnabledDevices.Select(getDeviceViewModel);
            DeviceViewModels = new ReadOnlyObservableCollection<LayoutFinderDeviceViewModel>(new ObservableCollection<LayoutFinderDeviceViewModel>(deviceGroups));
        });
    }

    public ReactiveCommand<Unit, Unit> SearchAll { get; }


    private async Task ExecuteSearchAll()
    {
        foreach (LayoutFinderDeviceViewModel deviceViewModel in DeviceViewModels)
        {
            try
            {
                await deviceViewModel.Search();
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to search for layout on device {Device}", deviceViewModel.Device);
            }
        }
    }
}