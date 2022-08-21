using System;
using Artemis.Core.Modules;

namespace Artemis.Core.Services;

/// <summary>
///     A service providing module activation functionality
/// </summary>
public interface IModuleService : IArtemisService
{
    /// <summary>
    ///     Updates all currently active modules
    /// </summary>
    /// <param name="deltaTime"></param>
    void UpdateActiveModules(double deltaTime);

    /// <summary>
    ///     Evaluates every enabled module's activation requirements and activates/deactivates modules accordingly
    /// </summary>
    void UpdateModuleActivation();

    /// <summary>
    ///     Overrides activation on the provided module and restores regular activation to any remaining modules
    /// </summary>
    void SetActivationOverride(Module? module);

    /// <summary>
    ///     Occurs whenever a module is activated
    /// </summary>
    event EventHandler<ModuleEventArgs> ModuleActivated;

    /// <summary>
    ///     Occurs whenever a module is deactivated
    /// </summary>
    event EventHandler<ModuleEventArgs> ModuleDeactivated;
}