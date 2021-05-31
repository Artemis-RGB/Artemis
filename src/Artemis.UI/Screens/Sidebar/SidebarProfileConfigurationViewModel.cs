using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Events;
using Artemis.UI.Screens.Sidebar.Dialogs.ProfileEdit;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.Sidebar
{
    public class SidebarProfileConfigurationViewModel : Screen
    {
        private readonly IDialogService _dialogService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IProfileService _profileService;
        private bool _isProfileActive;
        private bool _isSuspended;

        public SidebarProfileConfigurationViewModel(ProfileConfiguration profileConfiguration,
            IProfileService profileService,
            IDialogService dialogService,
            IEventAggregator eventAggregator)
        {
            _profileService = profileService;
            _dialogService = dialogService;
            _eventAggregator = eventAggregator;
            ProfileConfiguration = profileConfiguration;
            ProfileConfiguration.PropertyChanged += ProfileConfigurationOnPropertyChanged;
            Update();
        }

        public ProfileConfiguration ProfileConfiguration { get; }

        public bool IsProfileActive
        {
            get => _isProfileActive;
            set => SetAndNotify(ref _isProfileActive, value);
        }

        public bool IsSuspended
        {
            get => ProfileConfiguration.IsSuspended;
            set
            {
                ProfileConfiguration.IsSuspended = value;
                NotifyOfPropertyChange(nameof(IsSuspended));
                _profileService.SaveProfileCategory(ProfileConfiguration.Category);
            }
        }

        public async Task ViewProperties()
        {
            object result = await _dialogService.ShowDialog<ProfileEditViewModel>(new Dictionary<string, object>
            {
                {"profileConfiguration", ProfileConfiguration},
                {"isNew", false}
            });

            if (result is nameof(ProfileEditViewModel.Delete))
                await Delete();
        }

        public void Duplicate()
        {
            string export = _profileService.ExportProfile(ProfileConfiguration);
            _profileService.ImportProfile(ProfileConfiguration.Category, export, "copy");
        }

        public async Task Delete()
        {
            if (await _dialogService.ShowConfirmDialog("Delete profile", "Are you sure you want to delete this profile?\r\nThis cannot be undone."))
            {
                // Close the editor first by heading to Home if the profile is being edited
                if (ProfileConfiguration.IsBeingEdited)
                    _eventAggregator.Publish(new RequestSelectSidebarItemEvent("Home"));

                _profileService.RemoveProfileConfiguration(ProfileConfiguration);
            }
        }

        private void Update()
        {
            IsProfileActive = ProfileConfiguration.Profile != null;
        }

        #region Overrides of Screen

        /// <inheritdoc />
        protected override void OnDeactivate()
        {
            ProfileConfiguration.PropertyChanged -= ProfileConfigurationOnPropertyChanged;
            base.OnDeactivate();
        }

        #endregion

        #region Event handlers

        private void ProfileConfigurationOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ProfileConfiguration.Profile))
                Update();
        }

        #endregion
    }
}