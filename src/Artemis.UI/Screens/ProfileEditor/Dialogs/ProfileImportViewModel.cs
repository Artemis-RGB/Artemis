using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Core.Services;
using Artemis.UI.Shared.Services;
using ICSharpCode.AvalonEdit.Document;
using MaterialDesignThemes.Wpf;

namespace Artemis.UI.Screens.ProfileEditor.Dialogs
{
    public class ProfileImportViewModel : DialogViewModelBase
    {
        private readonly ISnackbarMessageQueue _mainMessageQueue;
        private readonly IProfileService _profileService;
        private string _profileJson;

        public ProfileImportViewModel(ProfileModule profileModule, IProfileService profileService, ISnackbarMessageQueue mainMessageQueue)
        {
            ProfileModule = profileModule;
            Document = new TextDocument();

            _profileService = profileService;
            _mainMessageQueue = mainMessageQueue;
        }

        public ProfileModule ProfileModule { get; }
        public TextDocument Document { get; set; }

        public string ProfileJson
        {
            get => _profileJson;
            set => SetAndNotify(ref _profileJson, value);
        }

        public void Accept()
        {
            ProfileDescriptor descriptor = _profileService.ImportProfile(Document.Text, ProfileModule);
            _mainMessageQueue.Enqueue("Profile imported.");
            Session.Close(descriptor);
        }
    }
}