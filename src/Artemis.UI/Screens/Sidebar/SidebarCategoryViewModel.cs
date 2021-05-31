using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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
                if (!SetAndNotify(ref _showItems, value)) return;

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

        public void OnMouseLeftButtonUp()
        {
            ShowItems = !ShowItems;
        }

        public async Task AddProfile()
        {
            ProfileConfiguration profileConfiguration = _profileService.CreateProfileConfiguration(ProfileCategory, "New profile", Enum.GetValues<PackIconKind>().First().ToString());
            object result = await _dialogService.ShowDialog<ProfileEditViewModel>(new Dictionary<string, object>
            {
                {"profileConfiguration", profileConfiguration},
                {"isNew", true}
            });
            if (result is nameof(ProfileEditViewModel.Accept))
                ShowItems = true;
            else
                _profileService.RemoveProfileConfiguration(profileConfiguration);
        }

        private void CreateProfileViewModels()
        {
            Items.Clear();
            foreach (ProfileConfiguration profileConfiguration in ProfileCategory.ProfileConfigurations.OrderBy(p => p.Order))
                Items.Add(_vmFactory.SidebarProfileConfigurationViewModel(profileConfiguration));
        }

        private void ProfileCategoryOnProfileConfigurationRemoved(object? sender, ProfileConfigurationEventArgs e)
        {
            if (ShowItems)
            {
                Items.Remove(Items.FirstOrDefault(i => i.ProfileConfiguration == e.ProfileConfiguration));
                ((BindableCollection<SidebarProfileConfigurationViewModel>) Items).Sort(p => p.ProfileConfiguration.Order);
            }
        }

        private void ProfileCategoryOnProfileConfigurationAdded(object? sender, ProfileConfigurationEventArgs e)
        {
            if (ShowItems)
            {
                Items.Add(_vmFactory.SidebarProfileConfigurationViewModel(e.ProfileConfiguration));
                ((BindableCollection<SidebarProfileConfigurationViewModel>)Items).Sort(p => p.ProfileConfiguration.Order);
            }
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

        #region Implementation of IDropTarget

        /// <inheritdoc />
        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Data is not SidebarProfileConfigurationViewModel || dropInfo.TargetItem is not SidebarProfileConfigurationViewModel) 
                return;

            dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
            dropInfo.Effects = DragDropEffects.Move;
        }

        /// <inheritdoc />
        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Data is not SidebarProfileConfigurationViewModel sourceItem || dropInfo.TargetItem is not SidebarProfileConfigurationViewModel targetItem) 
                return;
            if (sourceItem == targetItem)
                return;

            SidebarCategoryViewModel sourceCategory = (SidebarCategoryViewModel) sourceItem.Parent;
            SidebarCategoryViewModel targetCategory = (SidebarCategoryViewModel) targetItem.Parent;

            if (dropInfo.InsertPosition.HasFlag(RelativeInsertPosition.BeforeTargetItem))
                targetCategory.ProfileCategory.AddProfileConfiguration(sourceItem.ProfileConfiguration, targetCategory.Items.IndexOf(targetItem));
            else if (dropInfo.InsertPosition.HasFlag(RelativeInsertPosition.AfterTargetItem))
                targetCategory.ProfileCategory.AddProfileConfiguration(sourceItem.ProfileConfiguration, targetCategory.Items.IndexOf(targetItem) + 1);

            if (sourceCategory != targetCategory)
                _profileService.SaveProfileCategory(sourceCategory.ProfileCategory);
            _profileService.SaveProfileCategory(targetCategory.ProfileCategory);
        }

        #endregion
    }
}