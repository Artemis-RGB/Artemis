using System.Windows;
using System.Windows.Navigation;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.Screens.ProfileEditor.Dialogs
{
    public class ProfileExportViewModel : DialogViewModelBase
    {
        private readonly IProfileService _profileService;
        private readonly IMessageService _messageService;

        public ProfileExportViewModel(ProfileDescriptor profileDescriptor, IProfileService profileService, IMessageService messageService)
        {
            ProfileDescriptor = profileDescriptor;

            _profileService = profileService;
            _messageService = messageService;
        }

        public ProfileDescriptor ProfileDescriptor { get; }

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
            string encoded = _profileService.ExportProfile(ProfileDescriptor);
            Clipboard.SetText(encoded);
            _messageService.ShowMessage("Profile contents exported to clipboard.");

            Session.Close();
        }
    }
}