using DryIoc;

namespace Artemis.WebClient.Workshop.Handlers.InstallationHandlers;

public class EntryInstallationHandlerFactory
{
    private readonly IContainer _container;

    public EntryInstallationHandlerFactory(IContainer container)
    {
        _container = container;
    }

    public IEntryInstallationHandler CreateHandler(EntryType entryType)
    {
        return entryType switch
        {
            EntryType.Plugin => _container.Resolve<PluginEntryInstallationHandler>(),
            EntryType.Profile => _container.Resolve<ProfileEntryInstallationHandler>(),
            EntryType.Layout => _container.Resolve<LayoutEntryInstallationHandler>(),
            _ => throw new NotSupportedException($"EntryType '{entryType}' is not supported.")
        };
    }
}