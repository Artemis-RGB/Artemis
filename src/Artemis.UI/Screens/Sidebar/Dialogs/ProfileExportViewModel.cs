using System.Windows;
using System.Windows.Navigation;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.Screens.Sidebar.Dialogs
{
    public class ProfileExportViewModel : DialogViewModelBase
    {
        public ProfileConfiguration ProfileConfiguration { get; }
        private readonly IProfileService _profileService;
        private readonly IMessageService _messageService;

        public ProfileExportViewModel(ProfileConfiguration profileConfiguration, IProfileService profileService, IMessageService messageService)
        {
            ProfileConfiguration = profileConfiguration;

            _profileService = profileService;
            _messageService = messageService;
        }


        #region Overrides of Screen

        /// <inheritdoc />
        protected override void OnActivate()
        {
            // TODO: If the profile has hints on all layers, call Accept
            base.OnActivate();
        }

        #endregion

        public void OpenHyperlink(object sender, RequestNavigateEventArgs e)
        {
            Core.Utilities.OpenUrl(e.Uri.AbsoluteUri);
        }

        public void Accept()
        {
            string encoded = _profileService.ExportProfile(ProfileConfiguration);
            Clipboard.SetText(encoded);
            _messageService.ShowMessage("Profile contents exported to clipboard.");

            Session.Close();
        }
    }
}