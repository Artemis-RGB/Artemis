namespace Artemis.UI.Events
{
    public class RequestSelectSidebarItemEvent
    {
        public RequestSelectSidebarItemEvent(string label)
        {
            Label = label;
        }

        public string Label { get; }
    }
}