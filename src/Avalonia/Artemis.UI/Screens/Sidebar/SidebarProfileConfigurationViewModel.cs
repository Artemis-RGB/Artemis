using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.Interfaces;
using ReactiveUI;

namespace Artemis.UI.Screens.Sidebar
{
    public class SidebarProfileConfigurationViewModel : ActivatableViewModelBase
    {
        private readonly SidebarViewModel _sidebarViewModel;
        private readonly IProfileService _profileService;
        private readonly IWindowService _windowService;
        private ObservableAsPropertyHelper<bool>? _isSuspended;
        public ProfileConfiguration ProfileConfiguration { get; }

        public SidebarProfileConfigurationViewModel(SidebarViewModel sidebarViewModel, ProfileConfiguration profileConfiguration, IProfileService profileService, IWindowService windowService)
        {
            _sidebarViewModel = sidebarViewModel;
            _profileService = profileService;
            _windowService = windowService;

            ProfileConfiguration = profileConfiguration;

            this.WhenActivated(d =>
            {
                _isSuspended = ProfileConfiguration.WhenAnyValue(c => c.IsSuspended)
                    .ToProperty(this, vm => vm.IsSuspended)
                    .DisposeWith(d);
            });
            _profileService.LoadProfileConfigurationIcon(ProfileConfiguration);
        }

        public bool IsProfileActive => ProfileConfiguration.Profile != null;

        public bool IsSuspended
        {
            get => _isSuspended?.Value ?? false;
            set
            {
                ProfileConfiguration.IsSuspended = value;
                _profileService.SaveProfileCategory(ProfileConfiguration.Category);
            }
        }

        public async Task EditProfile()
        {
            if (await _windowService.ShowDialogAsync<ProfileConfigurationEditViewModel, bool>(("profileCategory", ProfileConfiguration.Category), ("profileConfiguration", ProfileConfiguration)))
                _sidebarViewModel.UpdateProfileCategories();
        }
    }
}