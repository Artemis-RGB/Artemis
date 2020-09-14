using System;

namespace Artemis.Core
{
    public class LayerPropertyGroupUpdatingEventArgs : EventArgs
    {
        public LayerPropertyGroupUpdatingEventArgs(double deltaTime)
        {
            DeltaTime = deltaTime;
        }

        public double DeltaTime { get; }
    }
}