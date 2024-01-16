using DryIoc;

namespace Artemis.WebClient.Workshop.Handlers.UploadHandlers;

public class EntryUploadHandlerFactory
{
    private readonly IContainer _container;

    public EntryUploadHandlerFactory(IContainer container)
    {
        _container = container;
    }

    public IEntryUploadHandler CreateHandler(EntryType entryType)
    {
        return entryType switch
        {
            EntryType.Profile => _container.Resolve<ProfileEntryUploadHandler>(),
            EntryType.Layout => _container.Resolve<LayoutEntryUploadHandler>(),
            _ => throw new NotSupportedException($"EntryType '{entryType}' is not supported.")
        };
    }
}