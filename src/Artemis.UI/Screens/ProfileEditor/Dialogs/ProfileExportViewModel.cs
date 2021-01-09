using System.Windows;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared.Services;
using MaterialDesignThemes.Wpf;

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

        public void Accept()
        {
            string encoded = _profileService.ExportProfile(ProfileDescriptor);
            Clipboard.SetText(encoded);
            _messageService.ShowMessage("Profile contents exported to clipboard.");

            Session.Close();
        }
    }
}