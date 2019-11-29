namespace Artemis.UI.Events
{
    public class MainWindowFocusChangedEvent
    {
        public MainWindowFocusChangedEvent(bool isFocused)
        {
            IsFocused = isFocused;
        }

        public bool IsFocused { get; }
    }
}