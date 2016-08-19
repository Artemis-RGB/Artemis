using System;

namespace Artemis.Events
{
    public class EnabledChangedEventArgs : EventArgs
    {
        public EnabledChangedEventArgs(bool enabled)
        {
            Enabled = enabled;
        }

        public bool Enabled { get; }
    }
}