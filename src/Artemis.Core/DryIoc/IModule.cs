using DryIoc;

namespace Artemis.Core.DryIoc;

/**
 * Represents a service module.
 */
public interface IModule
{
    /// <summary>
    /// Registers the services provided by the module.
    /// </summary>
    /// <param name="builder">The builder to register the services with.</param>
    void Load(IRegistrator builder);
}