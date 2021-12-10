using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.Interfaces;

namespace Artemis.UI.Screens.Sidebar
{
    public class SidebarProfileConfigurationViewModel : ViewModelBase
    {
        private readonly SidebarViewModel _sidebarViewModel;
        private readonly IProfileService _profileService;
        private readonly IWindowService _windowService;
        public ProfileConfiguration ProfileConfiguration { get; }

        public SidebarProfileConfigurationViewModel(SidebarViewModel sidebarViewModel, ProfileConfiguration profileConfiguration, IProfileService profileService, IWindowService windowService)
        {
            _sidebarViewModel = sidebarViewModel;
            _profileService = profileService;
            _windowService = windowService;
            ProfileConfiguration = profileConfiguration;

            _profileService.LoadProfileConfigurationIcon(ProfileConfiguration);
        }

        public bool IsProfileActive => ProfileConfiguration.Profile != null;

        public bool IsSuspended
        {
            get => ProfileConfiguration.IsSuspended;
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