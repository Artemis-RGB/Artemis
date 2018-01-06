using System;
using System.Threading.Tasks;
using Artemis.Core.Events;
using Artemis.Core.Modules.Interfaces;

namespace Artemis.Core.Services.Interfaces
{
    public interface IModuleService : IArtemisService, IDisposable
    {
        Task LoadModules();
        Task ReloadModule(IModule module);

        event EventHandler<ModuleEventArgs> ModuleLoaded;
        event EventHandler<ModuleEventArgs> ModuleReloaded;
    }
}