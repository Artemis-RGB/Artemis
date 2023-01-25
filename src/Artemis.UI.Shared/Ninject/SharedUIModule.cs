using System;
using Artemis.Core.DryIoc;
using Artemis.Core.Services;
using Artemis.UI.Shared.Services;
using DryIoc;

namespace Artemis.UI.Shared.Ninject;

/// <summary>
///     The main <see cref="IModule" /> of the Artemis Shared UI toolkit that binds all services
/// </summary>
public class SharedUIModule : IModule
{
    /// <inheritdoc />
    public void Load(IRegistrator builder)
    {
        var artemisShared = typeof(IArtemisSharedUIService).GetAssembly();
        
        builder.RegisterMany(new[] { artemisShared }, type => type.IsAssignableTo<IArtemisSharedUIService>(), Reuse.Singleton);
    }
}