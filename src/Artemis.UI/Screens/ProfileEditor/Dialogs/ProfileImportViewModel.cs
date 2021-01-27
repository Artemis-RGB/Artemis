using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Core.Services;
using Artemis.UI.Shared.Services;
using ICSharpCode.AvalonEdit.Document;

namespace Artemis.UI.Screens.ProfileEditor.Dialogs
{
    public class ProfileImportViewModel : DialogViewModelBase
    {
        private readonly IProfileService _profileService;
        private readonly IMessageService _messageService;
        private string _profileJson;

        public ProfileImportViewModel(ProfileModule profileModule, IProfileService profileService, IMessageService messageService)
        {
            ProfileModule = profileModule;
            Document = new TextDocument();

            _profileService = profileService;
            _messageService = messageService;
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
            _messageService.ShowMessage("Profile imported.");
            Session.Close(descriptor);
        }
    }
}