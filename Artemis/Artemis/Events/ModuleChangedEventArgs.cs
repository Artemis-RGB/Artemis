using System;
using Artemis.Modules.Abstract;

namespace Artemis.Events
{
    public class ModuleChangedEventArgs : EventArgs
    {
        public ModuleChangedEventArgs(ModuleModel module)
        {
            Module = module;
        }

        public ModuleModel Module { get; }
    }
}