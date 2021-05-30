using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.Sidebar.Dialogs;
using Artemis.UI.Shared.Services;
using MaterialDesignThemes.Wpf;
using Stylet;

namespace Artemis.UI.Screens.Sidebar
{
    public class SidebarCategoryViewModel : Conductor<SidebarProfileConfigurationViewModel>.Collection.AllActive
    {
        private readonly IDialogService _dialogService;
        private readonly ISidebarVmFactory _vmFactory;
        private readonly IProfileService _profileService;
        private SidebarProfileConfigurationViewModel _selectedProfileConfiguration;
        private bool _showItems;

        public SidebarCategoryViewModel(ProfileCategory profileCategory, IProfileService profileService, IDialogService dialogService, ISidebarVmFactory vmFactory)
        {
            _profileService = profileService;
            _dialogService = dialogService;
            _vmFactory = vmFactory;
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
            set
            {
                if (SetAndNotify(ref _selectedProfileConfiguration, value) && value != null)
                    ((SidebarViewModel) Parent).SelectProfileConfiguration(value.ProfileConfiguration);
            }
        }

        public async Task UpdateCategory()
        {
            object result = await _dialogService.ShowDialog<SidebarCategoryUpdateViewModel>(new Dictionary<string, object> {{"profileCategory", ProfileCategory}});
            if (result is true)
                await DeleteCategory();
        }

        public async Task DeleteCategory()
        {
            bool confirmed = await _dialogService.ShowConfirmDialog(
                "Delete category",
                "Are you sure you want to delete this category?\r\nAll profiles will be deleted too."
            );
            if (!confirmed)
                return;
            _profileService.DeleteProfileCategory(ProfileCategory);
            ((SidebarViewModel) Parent).RemoveProfileCategoryViewModel(this);
        }

        public void OnMouseLeftButtonUp()
        {
            ShowItems = !ShowItems;
        }

        public async Task AddProfile()
        {
            ProfileConfiguration profileConfiguration = _profileService.CreateProfileConfiguration(ProfileCategory, "New profile", Enum.GetValues<PackIconKind>().First().ToString());
            object save = await _dialogService.ShowDialog<ProfileEditViewModel>(new Dictionary<string, object> { { "profileConfiguration", profileConfiguration } });
            if (save is not true)
                _profileService.RemoveProfileConfiguration(profileConfiguration);
        }

        private void CreateProfileViewModels()
        {
            Items.Clear();
            foreach (ProfileConfiguration profileConfiguration in ProfileCategory.ProfileConfigurations)
                Items.Add(_vmFactory.SidebarProfileConfigurationViewModel(profileConfiguration));
        }

        private void ProfileCategoryOnProfileConfigurationRemoved(object? sender, ProfileConfigurationEventArgs e)
        {
            if (ShowItems)
                Items.Remove(Items.FirstOrDefault(i => i.ProfileConfiguration == e.ProfileConfiguration));
        }

        private void ProfileCategoryOnProfileConfigurationAdded(object? sender, ProfileConfigurationEventArgs e)
        {
            if (ShowItems)
                Items.Add(_vmFactory.SidebarProfileConfigurationViewModel(e.ProfileConfiguration));
        }

        #region Overrides of Screen

        /// <inheritdoc />
        protected override void OnInitialActivate()
        {
            ProfileCategory.ProfileConfigurationAdded += ProfileCategoryOnProfileConfigurationAdded;
            ProfileCategory.ProfileConfigurationRemoved += ProfileCategoryOnProfileConfigurationRemoved;
            base.OnInitialActivate();
        }

        /// <inheritdoc />
        protected override void OnClose()
        {
            ProfileCategory.ProfileConfigurationAdded -= ProfileCategoryOnProfileConfigurationAdded;
            ProfileCategory.ProfileConfigurationRemoved -= ProfileCategoryOnProfileConfigurationRemoved;
            base.OnClose();
        }

        #endregion
    }
}