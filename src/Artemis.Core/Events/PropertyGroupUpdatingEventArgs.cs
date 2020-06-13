using System;

namespace Artemis.Core.Events
{
    public class PropertyGroupUpdatingEventArgs : EventArgs
    {
        public PropertyGroupUpdatingEventArgs(double deltaTime)
        {
            DeltaTime = deltaTime;
        }

        public PropertyGroupUpdatingEventArgs(TimeSpan overrideTime)
        {
            OverrideTime = overrideTime;
        }

        public double DeltaTime { get; }
        public TimeSpan OverrideTime { get; }
    }
}