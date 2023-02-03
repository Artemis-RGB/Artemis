using Artemis.Core.DryIoc;
using Artemis.Core.Services;
using Artemis.UI.Linux.Providers.Input;
using DryIoc;

namespace Artemis.UI.Linux.DryIoc;

public class LinuxModule : IModule
{

    /// <inheritdoc />
    public void Load(IRegistrator builder)
    {
        builder.Register<InputProvider, LinuxInputProvider>(serviceKey: LinuxInputProvider.Id);
    }

}