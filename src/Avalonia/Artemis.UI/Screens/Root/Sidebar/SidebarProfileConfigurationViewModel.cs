using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.Root.Sidebar
{
    public class SidebarProfileConfigurationViewModel : ViewModelBase
    {
        private readonly IProfileService _profileService;
        public ProfileConfiguration ProfileConfiguration { get; }

        public SidebarProfileConfigurationViewModel(ProfileConfiguration profileConfiguration, IProfileService profileService)
        {
            _profileService = profileService;
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
    }
}