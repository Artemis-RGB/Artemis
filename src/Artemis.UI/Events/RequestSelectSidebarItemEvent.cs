namespace Artemis.UI.Events
{
    public class RequestSelectSidebarItemEvent
    {
        public RequestSelectSidebarItemEvent(string displayName)
        {
            DisplayName = displayName;
        }

        public string DisplayName { get; }
    }
}