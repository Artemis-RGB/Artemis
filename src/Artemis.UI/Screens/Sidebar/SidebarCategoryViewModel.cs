using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Events;
using Artemis.UI.Extensions;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.Sidebar.Dialogs;
using Artemis.UI.Screens.Sidebar.Dialogs.ProfileEdit;
using Artemis.UI.Shared.Services;
using GongSolutions.Wpf.DragDrop;
using MaterialDesignThemes.Wpf;
using MaterialDesignThemes.Wpf.Transitions;
using Newtonsoft.Json;
using Ookii.Dialogs.Wpf;
using Stylet;

namespace Artemis.UI.Screens.Sidebar
{
    public class SidebarCategoryViewModel : Conductor<SidebarProfileConfigurationViewModel>.Collection.AllActive, IDropTarget
    {
        private readonly IDialogService _dialogService;
        private readonly ISidebarVmFactory _vmFactory;
        private readonly IEventAggregator _eventAggregator;
        private readonly IProfileService _profileService;
        private SidebarProfileConfigurationViewModel _selectedProfileConfiguration;
        private bool _showItems;
        private readonly DefaultDropHandler _defaultDropHandler;
        private bool _addingProfile;

        public SidebarCategoryViewModel(ProfileCategory profileCategory,
            IProfileService profileService,
            IDialogService dialogService,
            ISidebarVmFactory vmFactory,
            IEventAggregator eventAggregator)
        {
            _profileService = profileService;
            _dialogService = dialogService;
            _vmFactory = vmFactory;
            _eventAggregator = eventAggregator;
            _showItems = !profileCategory.IsCollapsed;
            _defaultDropHandler = new DefaultDropHandler();

            ProfileCategory = profileCategory;
        }

        public ProfileCategory ProfileCategory { get; }

        public bool ShowItems
        {
            get => _showItems;
            set
            {
                if (!SetAndNotify(ref _showItems, value)) return;

                if (value)
                    CreateProfileViewModels();
                else
                    Items.Clear();
            }
        }

        public bool IsSuspended
        {
            get => ProfileCategory.IsSuspended;
            set
            {
                ProfileCategory.IsSuspended = value;
                NotifyOfPropertyChange(nameof(IsSuspended));
                _profileService.SaveProfileCategory(ProfileCategory);
            }
        }

        public SidebarProfileConfigurationViewModel SelectedProfileConfiguration
        {
            get => _selectedProfileConfiguration;
            set
            {
                if (!SetAndNotify(ref _selectedProfileConfiguration, value)) return;
                if (value == null) return;
                try
                {
                    ((SidebarViewModel) Parent).SelectProfileConfiguration(value.ProfileConfiguration);
                }
                catch (Exception e)
                {
                    _dialogService.ShowExceptionDialog("Failed select profile", e);
                    throw;
                }
            }
        }

        public async Task UpdateCategory()
        {
            object result = await _dialogService.ShowDialog<SidebarCategoryUpdateViewModel>(new Dictionary<string, object> {{"profileCategory", ProfileCategory}});
            if (result is nameof(SidebarCategoryUpdateViewModel.Delete))
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

            // Close the editor first by heading to Home if any of the profiles are being edited
            if (ProfileCategory.ProfileConfigurations.Any(p => p.IsBeingEdited))
                _eventAggregator.Publish(new RequestSelectSidebarItemEvent("Home"));

            _profileService.DeleteProfileCategory(ProfileCategory);
            ((SidebarViewModel) Parent).RemoveProfileCategoryViewModel(this);
        }

        public void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowItems = !ShowItems;
        }

        public async Task AddProfile()
        {
            try
            {
                _addingProfile = true;
                ProfileConfiguration profileConfiguration = _profileService.CreateProfileConfiguration(ProfileCategory, "New profile", Enum.GetValues<PackIconKind>().First().ToString());
                object result = await _dialogService.ShowDialog<ProfileEditViewModel>(new Dictionary<string, object>
                {
                    {"profileConfiguration", profileConfiguration},
                    {"isNew", true}
                });
                if (result is nameof(ProfileEditViewModel.Accept))
                {
                    if (!ShowItems)
                        ShowItems = true;
                    else
                        CreateProfileViewModels();
                }
                else
                    _profileService.RemoveProfileConfiguration(profileConfiguration);
            }
            finally
            {
                _addingProfile = false;
            }
        }

        public async Task ImportProfile()
        {
            VistaOpenFileDialog dialog = new()
            {
                Filter = "Artemis Profile|*.json",
                Title = "Export Artemis profile"
            };
            bool? result = dialog.ShowDialog();
            if (result != true)
                return;

            string json = await File.ReadAllTextAsync(dialog.FileName);
            try
            {
                ProfileConfigurationExportModel profileConfigurationExportModel = JsonConvert.DeserializeObject<ProfileConfigurationExportModel>(json, IProfileService.ExportSettings);
                if (profileConfigurationExportModel == null)
                {
                    await _dialogService.ShowConfirmDialog("Import profile", "Failed to import this profile, make sure it is a valid Artemis profile.", "Confirm", null);
                    return;
                }

                _profileService.ImportProfile(ProfileCategory, profileConfigurationExportModel);
            }
            catch (Exception e)
            {
                await _dialogService.ShowConfirmDialog("Import profile", $"Failed to import this profile, make sure it is a valid Artemis profile.\r\n{e.Message}", "Confirm", null);
            }
        }

        private void CreateProfileViewModels()
        {
            Items.Clear();
            foreach (ProfileConfiguration profileConfiguration in ProfileCategory.ProfileConfigurations.OrderBy(p => p.Order))
                Items.Add(_vmFactory.SidebarProfileConfigurationViewModel(profileConfiguration));

            SelectedProfileConfiguration = Items.FirstOrDefault(i => i.ProfileConfiguration.IsBeingEdited);
        }

        private void ProfileCategoryOnProfileConfigurationRemoved(object sender, ProfileConfigurationEventArgs e)
        {
            if (!_addingProfile && ShowItems)
            {
                Items.Remove(Items.FirstOrDefault(i => i.ProfileConfiguration == e.ProfileConfiguration));
                ((BindableCollection<SidebarProfileConfigurationViewModel>) Items).Sort(p => p.ProfileConfiguration.Order);
            }

            SelectedProfileConfiguration = Items.FirstOrDefault(i => i.ProfileConfiguration.IsBeingEdited);
        }

        private void ProfileCategoryOnProfileConfigurationAdded(object sender, ProfileConfigurationEventArgs e)
        {
            if (!_addingProfile && ShowItems)
            {
                Items.Add(_vmFactory.SidebarProfileConfigurationViewModel(e.ProfileConfiguration));
                ((BindableCollection<SidebarProfileConfigurationViewModel>) Items).Sort(p => p.ProfileConfiguration.Order);
            }

            SelectedProfileConfiguration = Items.FirstOrDefault(i => i.ProfileConfiguration.IsBeingEdited);
        }

        #region Overrides of Screen

        #region Overrides of AllActive

        /// <inheritdoc />
        protected override void OnActivate()
        {
            CreateProfileViewModels();
            base.OnActivate();
        }

        #endregion

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

        #region Implementation of IDropTarget

        /// <inheritdoc />
        public void DragOver(IDropInfo dropInfo)
        {
            _defaultDropHandler.DragOver(dropInfo);
        }

        /// <inheritdoc />
        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Data is not SidebarProfileConfigurationViewModel sourceItem)
                return;
            if (dropInfo.Data == dropInfo.TargetItem)
                return;

            SidebarCategoryViewModel sourceCategory = (SidebarCategoryViewModel) sourceItem.Parent;
            SidebarCategoryViewModel targetCategory = (SidebarCategoryViewModel) ((FrameworkElement) dropInfo.VisualTarget).DataContext;

            int targetIndex = 0;
            if (dropInfo.TargetItem is SidebarProfileConfigurationViewModel targetItem)
            {
                if (dropInfo.InsertPosition.HasFlag(RelativeInsertPosition.BeforeTargetItem))
                    targetIndex = targetCategory.Items.IndexOf(targetItem);
                else
                    targetIndex = targetCategory.Items.IndexOf(targetItem) + 1;
            }

            targetCategory.ProfileCategory.AddProfileConfiguration(sourceItem.ProfileConfiguration, targetIndex);

            if (sourceCategory != targetCategory)
                _profileService.SaveProfileCategory(sourceCategory.ProfileCategory);
            _profileService.SaveProfileCategory(targetCategory.ProfileCategory);
        }

        #endregion
    }
}