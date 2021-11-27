using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.Root.Sidebar.Dialogs;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.Interfaces;
using FluentAvalonia.UI.Controls;
using ReactiveUI;

namespace Artemis.UI.Screens.Root.Sidebar
{
    public class SidebarCategoryViewModel : ViewModelBase
    {
        private readonly SidebarViewModel _sidebarViewModel;
        private readonly IProfileService _profileService;
        private readonly IWindowService _windowService;
        private readonly ISidebarVmFactory _vmFactory;
        private SidebarProfileConfigurationViewModel? _selectedProfileConfiguration;

        public SidebarCategoryViewModel(SidebarViewModel sidebarViewModel, ProfileCategory profileCategory, IProfileService profileService, IWindowService windowService, ISidebarVmFactory vmFactory)
        {
            _sidebarViewModel = sidebarViewModel;
            _profileService = profileService;
            _windowService = windowService;
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

        public async Task EditCategory()
        {
            await _windowService.CreateContentDialog()
                .WithTitle("Edit category")
                .WithViewModel<SidebarCategoryCreateViewModel>(out var vm, ("category", ProfileCategory))
                .HavingPrimaryButton(b => b.WithText("Confirm").WithCommand(vm.Confirm))
                .HavingSecondaryButton(b => b.WithText("Delete").WithCommand(vm.Delete))
                .WithDefaultButton(ContentDialogButton.Primary)
                .ShowAsync();

            _sidebarViewModel.UpdateProfileCategories();
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