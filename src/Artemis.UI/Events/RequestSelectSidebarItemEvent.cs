using Artemis.UI.Screens.Sidebar;

namespace Artemis.UI.Events
{
    public class RequestSelectSidebarItemEvent
    {
        public RequestSelectSidebarItemEvent(string displayName)
        {
            DisplayName = displayName;
        }

        public RequestSelectSidebarItemEvent(SidebarScreenViewModel viewModel)
        {
            ViewModel = viewModel;
        }

        public string DisplayName { get; }
        public SidebarScreenViewModel ViewModel { get; }
    }
}