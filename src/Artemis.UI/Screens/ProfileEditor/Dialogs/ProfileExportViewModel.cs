using System.Windows;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared.Services;
using MaterialDesignThemes.Wpf;

namespace Artemis.UI.Screens.ProfileEditor.Dialogs
{
    public class ProfileExportViewModel : DialogViewModelBase
    {
        private readonly ISnackbarMessageQueue _mainMessageQueue;

        private readonly IProfileService _profileService;

        public ProfileExportViewModel(ProfileDescriptor profileDescriptor, IProfileService profileService, ISnackbarMessageQueue mainMessageQueue)
        {
            ProfileDescriptor = profileDescriptor;

            _profileService = profileService;
            _mainMessageQueue = mainMessageQueue;
        }

        public ProfileDescriptor ProfileDescriptor { get; }

        public void Accept()
        {
            string encoded = _profileService.ExportProfile(ProfileDescriptor);
            Clipboard.SetText(encoded);
            _mainMessageQueue.Enqueue("Profile contents exported to clipboard.");

            Session.Close();
        }

        public void Cancel()
        {
            Session.Close();
        }
    }
}