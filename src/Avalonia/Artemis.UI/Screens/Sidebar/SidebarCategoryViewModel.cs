using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.Builders;
using Artemis.UI.Shared.Services.Interfaces;
using Artemis.UI.Shared.Services.ProfileEditor;
using ReactiveUI;

namespace Artemis.UI.Screens.Sidebar
{
    public class SidebarCategoryViewModel : ActivatableViewModelBase
    {
        private readonly IProfileService _profileService;
        private readonly SidebarViewModel _sidebarViewModel;
        private readonly ISidebarVmFactory _vmFactory;
        private readonly IWindowService _windowService;
        private SidebarProfileConfigurationViewModel? _selectedProfileConfiguration;

        public SidebarCategoryViewModel(SidebarViewModel sidebarViewModel, ProfileCategory profileCategory, IProfileService profileService, IWindowService windowService,
            IProfileEditorService profileEditorService, ISidebarVmFactory vmFactory)
        {
            _sidebarViewModel = sidebarViewModel;
            _profileService = profileService;
            _windowService = windowService;
            _vmFactory = vmFactory;

            ProfileCategory = profileCategory;

            if (ShowItems)
                CreateProfileViewModels();

            this.WhenActivated(disposables =>
            {
                profileEditorService.ProfileConfiguration
                    .Subscribe(p => SelectedProfileConfiguration = ProfileConfigurations.FirstOrDefault(c => ReferenceEquals(c.ProfileConfiguration, p)))
                    .DisposeWith(disposables);
                this.WhenAnyValue(vm => vm.SelectedProfileConfiguration)
                    .WhereNotNull()
                    .Subscribe(s => profileEditorService.ChangeCurrentProfileConfiguration(s.ProfileConfiguration));
            });
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
                .WithViewModel(out SidebarCategoryEditViewModel vm, ("category", ProfileCategory))
                .HavingPrimaryButton(b => b.WithText("Confirm").WithCommand(vm.Confirm))
                .HavingSecondaryButton(b => b.WithText("Delete").WithCommand(vm.Delete))
                .WithCloseButtonText("Cancel")
                .WithDefaultButton(ContentDialogButton.Primary)
                .ShowAsync();

            _sidebarViewModel.UpdateProfileCategories();
        }

        public async Task AddProfile()
        {
            bool result = await _windowService.ShowDialogAsync<ProfileConfigurationEditViewModel, bool>(("profileCategory", ProfileCategory), ("profileConfiguration", null));
            if (result)
                _sidebarViewModel.UpdateProfileCategories();
        }

        private void CreateProfileViewModels()
        {
            ProfileConfigurations.Clear();
            foreach (ProfileConfiguration profileConfiguration in ProfileCategory.ProfileConfigurations.OrderBy(p => p.Order))
                ProfileConfigurations.Add(_vmFactory.SidebarProfileConfigurationViewModel(_sidebarViewModel, profileConfiguration));

            SelectedProfileConfiguration = ProfileConfigurations.FirstOrDefault(i => i.ProfileConfiguration.IsBeingEdited);
        }
    }
}