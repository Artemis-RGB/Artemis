namespace Artemis.UI.Events
{
    public class MainWindowFocusChanged
    {
        public MainWindowFocusChanged(bool isFocused)
        {
            IsFocused = isFocused;
        }

        public bool IsFocused { get; }
    }
}