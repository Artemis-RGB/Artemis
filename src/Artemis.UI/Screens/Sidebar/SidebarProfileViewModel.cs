using Artemis.Core;
using Stylet;

namespace Artemis.UI.Screens.Sidebar
{
    public class SidebarProfileViewModel : Screen
    {
        public ProfileDescriptor ProfileDescriptor { get; }

        public SidebarProfileViewModel(ProfileDescriptor profileDescriptor)
        {
            ProfileDescriptor = profileDescriptor;
        }
    }
}