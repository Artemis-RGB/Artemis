using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Profile;

public class ProfilePreviewViewModel : ActivatableViewModelBase
{
    private readonly IProfileService _profileService;
    private readonly IWindowService _windowService;
    private ProfileConfiguration? _profileConfiguration;

    public ProfilePreviewViewModel(IProfileService profileService, IDeviceService deviceService, IWindowService windowService)
    {
        _profileService = profileService;
        _windowService = windowService;

        Devices = new ObservableCollection<ArtemisDevice>(deviceService.EnabledDevices.OrderBy(d => d.ZIndex));

        this.WhenAnyValue(vm => vm.ProfileConfiguration).Subscribe(_ => Update());
        this.WhenActivated(d => Disposable.Create(() => PreviewProfile(null)).DisposeWith(d));
    }

    public ObservableCollection<ArtemisDevice> Devices { get; }

    public ProfileConfiguration? ProfileConfiguration
    {
        get => _profileConfiguration;
        set => RaiseAndSetIfChanged(ref _profileConfiguration, value);
    }

    private void Update()
    {
        try
        {
            PreviewProfile(ProfileConfiguration);
        }
        catch (Exception e)
        {
            _windowService.ShowExceptionDialog("Failed to load preview", e);
        }
    }

    private void PreviewProfile(ProfileConfiguration? profileConfiguration)
    {
        _profileService.FocusProfile = profileConfiguration;
        _profileService.UpdateFocusProfile = profileConfiguration != null;
    }
}