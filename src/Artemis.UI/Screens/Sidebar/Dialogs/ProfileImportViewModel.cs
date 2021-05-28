using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.Screens.Sidebar.Dialogs
{
    public class ProfileImportViewModel : DialogViewModelBase
    {
        public ProfileCategory Category { get; }
        private readonly IProfileService _profileService;
        private readonly IMessageService _messageService;
        private string _profileJson;

        public ProfileImportViewModel(ProfileCategory category, IProfileService profileService, IMessageService messageService)
        {
            Category = category;

            _profileService = profileService;
            _messageService = messageService;
        }


        public string ProfileJson
        {
            get => _profileJson;
            set => SetAndNotify(ref _profileJson, value);
        }

        public void Accept()
        {
            ProfileConfiguration descriptor = _profileService.ImportProfile(Category, ProfileJson);
            _messageService.ShowMessage("Profile imported.");
            Session.Close(descriptor);
        }
    }
}