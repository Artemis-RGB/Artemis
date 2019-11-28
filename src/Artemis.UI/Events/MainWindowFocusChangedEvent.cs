using System;

namespace Artemis.UI.Events
{
    public class MainWindowFocusChangedEvent : EventArgs
    {
        public MainWindowFocusChangedEvent(bool isFocused)
        {
            IsFocused = isFocused;
        }

        public bool IsFocused { get; }
    }
}