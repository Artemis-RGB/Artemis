using System;
using Artemis.Core.Models;

namespace Artemis.Core.Exceptions
{
    public class ArtemisModuleException : Exception
    {
        public ArtemisModuleException(ModuleInfo moduleInfo)
        {
            ModuleInfo = moduleInfo;
        }

        public ArtemisModuleException(ModuleInfo moduleInfo, string message) : base(message)
        {
            ModuleInfo = moduleInfo;
        }

        public ArtemisModuleException(ModuleInfo moduleInfo, string message, Exception inner) : base(message, inner)
        {
            ModuleInfo = moduleInfo;
        }

        public ModuleInfo ModuleInfo { get; }
    }
}