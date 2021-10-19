using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Avalonia.Ninject.Factories;
using ReactiveUI;

namespace Artemis.UI.Avalonia.Screens.Root.ViewModels
{
    public class SidebarCategoryViewModel : ViewModelBase
    {
        private readonly IProfileService _profileService;
        private readonly ISidebarVmFactory _vmFactory;
        private SidebarProfileConfigurationViewModel? _selectedProfileConfiguration;

        public SidebarCategoryViewModel(ProfileCategory profileCategory, IProfileService profileService, ISidebarVmFactory vmFactory)
        {
            _profileService = profileService;
            _vmFactory = vmFactory;

            ProfileCategory = profileCategory;

            if (ShowItems)
                CreateProfileViewModels();
        }

        public ProfileCategory ProfileCategory { get; }

        public ObservableCollection<SidebarProfileConfigurationViewModel> ProfileConfigurations { get; } = new();

        public SidebarProfileConfigurationViewModel? SelectedProfileConfiguration
        {
            get => _selectedProfileConfiguration;
            set => this.RaiseAndSetIfChanged(ref _selectedProfileConfiguration, value);
        }

        public bool ShowItems
        {
            get => !ProfileCategory.IsCollapsed;
            set
            {
                ProfileCategory.IsCollapsed = !value;
                if (ProfileCategory.IsCollapsed)
                    ProfileConfigurations.Clear();
                else
                    CreateProfileViewModels();
                _profileService.SaveProfileCategory(ProfileCategory);

                this.RaisePropertyChanged(nameof(ShowItems));
            }
        }

        public bool IsSuspended
        {
            get => ProfileCategory.IsSuspended;
            set
            {
                ProfileCategory.IsSuspended = value;
                this.RaisePropertyChanged(nameof(IsSuspended));
                _profileService.SaveProfileCategory(ProfileCategory);
            }
        }

        private void CreateProfileViewModels()
        {
            ProfileConfigurations.Clear();
            foreach (ProfileConfiguration profileConfiguration in ProfileCategory.ProfileConfigurations.OrderBy(p => p.Order))
                ProfileConfigurations.Add(_vmFactory.SidebarProfileConfigurationViewModel(profileConfiguration));

            SelectedProfileConfiguration = ProfileConfigurations.FirstOrDefault(i => i.ProfileConfiguration.IsBeingEdited);
        }
    }
}