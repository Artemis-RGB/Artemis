using System;
using System.Threading.Tasks;
using Artemis.Core.Modules;

namespace Artemis.Core.Services
{
    /// <summary>
    ///     A service providing module activation functionality
    /// </summary>
    public interface IModuleService : IArtemisService
    {
        /// <summary>
        ///     Occurs when the priorty of a module is updated.
        /// </summary>
        event EventHandler ModulePriorityUpdated;

        /// <summary>
        ///     Gets the current active module override. If set, all other modules are deactivated and only the
        ///     <see cref="ActiveModuleOverride" /> is active.
        /// </summary>
        Module? ActiveModuleOverride { get; }

        /// <summary>
        ///     Changes the current <see cref="ActiveModuleOverride" /> and deactivates all other modules
        /// </summary>
        /// <param name="overrideModule"></param>
        Task SetActiveModuleOverride(Module? overrideModule);

        /// <summary>
        ///     Evaluates every enabled module's activation requirements and activates/deactivates modules accordingly
        /// </summary>
        Task UpdateModuleActivation();

        /// <summary>
        ///     Updates the priority and priority category of the given module
        /// </summary>
        /// <param name="module">The module to update</param>
        /// <param name="category">The new priority category of the module</param>
        /// <param name="priority">The new priority of the module</param>
        void UpdateModulePriority(Module module, ModulePriorityCategory category, int priority);
    }
}