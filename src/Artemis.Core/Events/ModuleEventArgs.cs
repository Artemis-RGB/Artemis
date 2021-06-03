using System;
using Artemis.Core.Modules;

namespace Artemis.Core
{
    /// <summary>
    ///     Provides data about module events
    /// </summary>
    public class ModuleEventArgs : EventArgs
    {
        internal ModuleEventArgs(Module module)
        {
            Module = module;
        }

        /// <summary>
        ///     Gets the module this event is related to
        /// </summary>
        public Module Module { get; }
    }
}