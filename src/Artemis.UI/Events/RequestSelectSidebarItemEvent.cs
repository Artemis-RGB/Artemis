namespace Artemis.UI.Events
{
    public class RequestSelectSidebarItemEvent
    {
        public string Label { get; }

        public RequestSelectSidebarItemEvent(string label)
        {
            Label = label;
        }
    }
}