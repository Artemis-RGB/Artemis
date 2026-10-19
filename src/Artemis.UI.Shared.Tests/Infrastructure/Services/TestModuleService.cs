using Artemis.Core.Modules;
using Artemis.Core.Services;

namespace Artemis.UI.Shared.Tests.Infrastructure.Services;

/// <summary>
///     Records the activation overrides the profile editor sets.
/// </summary>
public class TestModuleService : IModuleService
{
    /// <summary>
    ///     Gets every module passed to <see cref="SetActivationOverride" />, in order
    /// </summary>
    public List<Module?> ActivationOverrides { get; } = [];

    public void SetActivationOverride(Module? module)
    {
        ActivationOverrides.Add(module);
    }

    public void UpdateActiveModules(double deltaTime) => throw new NotSupportedException();
    public void UpdateModuleActivation() => throw new NotSupportedException();

#pragma warning disable CS0067 // Events are part of the interface but never raised here
    public event EventHandler<ModuleEventArgs>? ModuleActivated;
    public event EventHandler<ModuleEventArgs>? ModuleDeactivated;
#pragma warning restore CS0067
}
