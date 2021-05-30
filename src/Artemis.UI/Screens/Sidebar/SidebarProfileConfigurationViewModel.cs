using System.Collections.Generic;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Screens.Sidebar.Dialogs;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.Sidebar
{
    public class SidebarProfileConfigurationViewModel : Screen
    {
        private readonly IDialogService _dialogService;
        public ProfileConfiguration ProfileConfiguration { get; }

        public SidebarProfileConfigurationViewModel(ProfileConfiguration profileConfiguration, IDialogService dialogService)
        {
            _dialogService = dialogService;
            ProfileConfiguration = profileConfiguration;
        }

        public async Task ViewProperties()
        {
            await _dialogService.ShowDialog<ProfileEditViewModel>(new Dictionary<string, object> {{"profileConfiguration", ProfileConfiguration}});
        }
    }
}