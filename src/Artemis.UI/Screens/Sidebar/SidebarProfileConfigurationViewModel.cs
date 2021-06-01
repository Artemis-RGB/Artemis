using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Events;
using Artemis.UI.Screens.Sidebar.Dialogs.ProfileEdit;
using Artemis.UI.Screens.Sidebar.Models;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.Sidebar
{
    public class SidebarProfileConfigurationViewModel : Screen
    {
        private readonly IDialogService _dialogService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IProfileService _profileService;

        public SidebarProfileConfigurationViewModel(ProfileConfiguration profileConfiguration,
            IProfileService profileService,
            IDialogService dialogService,
            IEventAggregator eventAggregator)
        {
            _profileService = profileService;
            _dialogService = dialogService;
            _eventAggregator = eventAggregator;
            ProfileConfiguration = profileConfiguration;
        }

        public ProfileConfiguration ProfileConfiguration { get; }

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

        public void SuspendAbove(string action)
        {
            foreach (ProfileConfiguration profileConfiguration in ProfileConfiguration.Category.ProfileConfigurations.OrderBy(p => p.Order).TakeWhile(c => c != ProfileConfiguration))
            {
                if (profileConfiguration != ProfileConfiguration)
                    profileConfiguration.IsSuspended = action == "suspend";
            }
        }

        public void SuspendBelow(string action)
        {
            foreach (ProfileConfiguration profileConfiguration in ProfileConfiguration.Category.ProfileConfigurations.OrderBy(p => p.Order).SkipWhile(c => c != ProfileConfiguration))
            {
                if (profileConfiguration != ProfileConfiguration)
                    profileConfiguration.IsSuspended = action == "suspend";
            }
        }

        public void Duplicate()
        {
            string export = _profileService.ExportProfile(ProfileConfiguration);
            _profileService.ImportProfile(ProfileConfiguration.Category, export, "copy");
        }

        public void Copy()
        {
            // The profile may not be active and in that case lets activate it real quick
            Profile profile = ProfileConfiguration.Profile ?? _profileService.ActivateProfile(ProfileConfiguration);
            JsonClipboard.SetObject(new ProfileConfigurationClipboardModel
            {
                ProfileConfigurationEntity = ProfileConfiguration.Entity,
                ProfileEntity = profile.ProfileEntity
            });
        }

        public void Paste()
        {
            ProfileConfigurationClipboardModel profileConfiguration = JsonClipboard.GetData<ProfileConfigurationClipboardModel>();
            if (profileConfiguration == null)
                return;

            // _profileService.Im
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

        #region Overrides of Screen

        /// <inheritdoc />
        protected override void OnActivate()
        {
            _profileService.LoadProfileConfigurationIcon(ProfileConfiguration);
            ProfileConfiguration.PropertyChanged += ProfileConfigurationOnPropertyChanged;
            NotifyOfPropertyChange(nameof(IsProfileActive));

            base.OnActivate();
        }

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
                NotifyOfPropertyChange(nameof(IsProfileActive));
            if (e.PropertyName == nameof(ProfileConfiguration.IsSuspended))
                NotifyOfPropertyChange(nameof(IsSuspended));
        }

        #endregion
    }
}