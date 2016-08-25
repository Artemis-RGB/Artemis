using System;
using Artemis.Models;

namespace Artemis.Events
{
    public class EffectChangedEventArgs : EventArgs
    {
        public EffectChangedEventArgs(EffectModel effect)
        {
            Effect = effect;
        }

        public EffectModel Effect { get; }
    }
}