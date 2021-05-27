using System.Collections.Generic;
using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Core.Services;
using Stylet;

namespace Artemis.UI.Screens.Sidebar
{
    public class SidebarCategoryViewModel : Conductor<SidebarProfileConfigurationViewModel>.Collection.AllActive
    {
        private readonly IPluginManagementService _pluginManagementService;
        private readonly IProfileService _profileService;
        private SidebarProfileConfigurationViewModel _selectedProfileConfiguration;
        private bool _showItems;

        public SidebarCategoryViewModel(ProfileCategory profileCategory, IPluginManagementService pluginManagementService, IProfileService profileService)
        {
            _pluginManagementService = pluginManagementService;
            _profileService = profileService;
            _showItems = !profileCategory.IsCollapsed;

            ProfileCategory = profileCategory;
            if (ShowItems)
                CreateProfileViewModels();
        }

        public ProfileCategory ProfileCategory { get; }

        public bool ShowItems
        {
            get => _showItems;
            set
            {
                SetAndNotify(ref _showItems, value);
                if (value)
                    CreateProfileViewModels();
                else
                    Items.Clear();
            }
        }

        public SidebarProfileConfigurationViewModel SelectedProfileConfiguration
        {
            get => _selectedProfileConfiguration;
            set => SetAndNotify(ref _selectedProfileConfiguration, value);
        }

        public void OnMouseLeftButtonUp()
        {
            ShowItems = !ShowItems;
        }
        
        private void CreateProfileViewModels()
        {
            Items.Clear();
            List<ProfileModule> featuresOfType = _pluginManagementService.GetFeaturesOfType<ProfileModule>();

            foreach (ProfileModule profileModule in featuresOfType)
            foreach (ProfileDescriptor profileDescriptor in _profileService.GetProfileDescriptors(profileModule))
                Items.Add(new SidebarProfileConfigurationViewModel(profileDescriptor));
        }
    }
}