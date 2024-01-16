using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Artemis.Core;
using Artemis.Core.DeviceProviders;
using Artemis.Core.Services;
using Artemis.UI.Screens.Workshop.Layout.Dialogs;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop.Handlers.UploadHandlers;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;

namespace Artemis.UI.Screens.Workshop.Layout;

public partial class LayoutInfoViewModel : ValidatableViewModelBase
{
    private readonly IWindowService _windowService;
    private readonly ObservableAsPropertyHelper<string?> _deviceProviders;
    [Notify] private string? _vendor;
    [Notify] private string? _model;
    [Notify] private Guid _deviceProviderId;
    [Notify] private string? _deviceProviderIdInput;
    [Notify] private ICommand? _remove;

    public LayoutInfoViewModel(ArtemisLayout layout,
        IDeviceService deviceService,
        IWindowService windowService,
        IPluginManagementService pluginManagementService)
    {
        _windowService = windowService;
        _vendor = layout.RgbLayout.Vendor;
        _model = layout.RgbLayout.Model;

        DeviceProvider? deviceProvider = deviceService.Devices.FirstOrDefault(d => d.Layout == layout)?.DeviceProvider;
        if (deviceProvider != null)
            _deviceProviderId = deviceProvider.Plugin.Guid;

        _deviceProviders = this.WhenAnyValue(vm => vm.DeviceProviderId)
            .Select(id => pluginManagementService.GetAllPlugins().FirstOrDefault(p => p.Guid == id)?.Features.Select(f => f.Name))
            .Select(names => names != null ? string.Join(", ", names) : "")
            .ToProperty(this, vm => vm.DeviceProviders);

        this.WhenAnyValue(vm => vm.DeviceProviderId).Subscribe(g => DeviceProviderIdInput = g.ToString());
        this.WhenAnyValue(vm => vm.DeviceProviderIdInput).Where(i => Guid.TryParse(i, out _)).Subscribe(i => DeviceProviderId = Guid.Parse(i!));

        this.ValidationRule(vm => vm.Model, input => !string.IsNullOrWhiteSpace(input), "Device model is required");
        this.ValidationRule(vm => vm.Vendor, input => !string.IsNullOrWhiteSpace(input), "Device vendor is required");
        this.ValidationRule(vm => vm.DeviceProviderIdInput, input => Guid.TryParse(input, out _), "Must be a valid GUID formatted as: 00000000-0000-0000-0000-000000000000");
        this.ValidationRule(vm => vm.DeviceProviderIdInput, input => !string.IsNullOrWhiteSpace(input), "Device provider ID is required");
        this.ValidationRule(vm => vm.DeviceProviderIdInput, input => input != "00000000-0000-0000-0000-000000000000", "Device provider ID is required");
    }

    public string? DeviceProviders => _deviceProviders.Value;

    public async Task BrowseDeviceProvider()
    {
        await _windowService.CreateContentDialog()
            .WithTitle("Select device provider")
            .WithViewModel(out DeviceProviderPickerDialogViewModel vm)
            .ShowAsync();

        DeviceProvider? deviceProvider = vm.DeviceProvider;
        if (deviceProvider != null)
            DeviceProviderId = deviceProvider.Plugin.Guid;
    }

    public LayoutInfo ToLayoutInfo()
    {
        return new LayoutInfo
        {
            Model = Model ?? string.Empty,
            Vendor = Vendor ?? string.Empty,
            DeviceProviderId = DeviceProviderId
        };
    }
}