using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Avalonia.Shared;

namespace Artemis.UI.Avalonia.Screens.Root.ViewModels
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