using Artemis.Core;
using Stylet;

namespace Artemis.UI.Screens.Sidebar
{
    public class SidebarProfileConfigurationViewModel : Screen
    {
        public ProfileConfiguration ProfileConfiguration { get; }
        
        public SidebarProfileConfigurationViewModel(ProfileConfiguration profileConfiguration)
        {
            ProfileConfiguration = profileConfiguration;
        }
    }
}