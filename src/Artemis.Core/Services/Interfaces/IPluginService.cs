using System;
using System.Threading.Tasks;
using Artemis.Core.Events;
using Artemis.Plugins.Interfaces;

namespace Artemis.Core.Services.Interfaces
{
    public interface IPluginService : IArtemisService, IDisposable
    {
        Task LoadModules();
        Task ReloadModule(IPlugin plugin);

        event EventHandler<ModuleEventArgs> ModuleLoaded;
        event EventHandler<ModuleEventArgs> ModuleReloaded;
    }
}