using System.Linq;
using Artemis.Core;
using Stylet;

namespace Artemis.UI.Screens.Sidebar
{
    public class SidebarCategoryViewModel : Conductor<SidebarProfileConfigurationViewModel>.Collection.AllActive
    {
        private SidebarProfileConfigurationViewModel _selectedProfileConfiguration;
        private bool _showItems;

        public SidebarCategoryViewModel(ProfileCategory profileCategory)
        {
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

        #region Overrides of Screen

        /// <inheritdoc />
        protected override void OnInitialActivate()
        {
            ProfileCategory.ProfileConfigurationAdded -= ProfileCategoryOnProfileConfigurationAdded;
            ProfileCategory.ProfileConfigurationRemoved -= ProfileCategoryOnProfileConfigurationRemoved;
            base.OnInitialActivate();
        }

        /// <inheritdoc />
        protected override void OnClose()
        {
            ProfileCategory.ProfileConfigurationAdded += ProfileCategoryOnProfileConfigurationAdded;
            ProfileCategory.ProfileConfigurationRemoved += ProfileCategoryOnProfileConfigurationRemoved;
            base.OnClose();
        }

        #endregion

        private void CreateProfileViewModels()
        {
            Items.Clear();
            foreach (ProfileConfiguration profileConfiguration in ProfileCategory.ProfileConfigurations)
                Items.Add(new SidebarProfileConfigurationViewModel(profileConfiguration));
        }

        private void ProfileCategoryOnProfileConfigurationRemoved(object? sender, ProfileConfigurationEventArgs e)
        {
            if (ShowItems)
                Items.Remove(Items.FirstOrDefault(i => i.ProfileConfiguration == e.ProfileConfiguration));
        }

        private void ProfileCategoryOnProfileConfigurationAdded(object? sender, ProfileConfigurationEventArgs e)
        {
            if (ShowItems)
                Items.Add(new SidebarProfileConfigurationViewModel(e.ProfileConfiguration));
        }
    }
}