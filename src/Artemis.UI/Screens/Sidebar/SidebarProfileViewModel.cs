using Artemis.Core;
using Stylet;

namespace Artemis.UI.Screens.Sidebar
{
    public class SidebarProfileViewModel : Screen
    {
        private bool _isProfileActive;
        public ProfileDescriptor ProfileDescriptor { get; }

        public bool IsProfileActive
        {
            get => _isProfileActive;
            set => SetAndNotify(ref _isProfileActive, value);
        }
        
        public SidebarProfileViewModel(ProfileDescriptor profileDescriptor)
        {
            ProfileDescriptor = profileDescriptor;
        }
    }
}