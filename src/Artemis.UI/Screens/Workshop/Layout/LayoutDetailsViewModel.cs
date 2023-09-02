using System;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Screens.Workshop.Parameters;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using Artemis.WebClient.Workshop;
using StrawberryShake;

namespace Artemis.UI.Screens.Workshop.Layout;

public class LayoutDetailsViewModel : RoutableScreen<WorkshopDetailParameters>
{
    private readonly IWorkshopClient _client;
    private IGetEntryById_Entry? _entry;

    public LayoutDetailsViewModel(IWorkshopClient client)
    {
        _client = client;
    }

    public EntryType? EntryType => null;

    public IGetEntryById_Entry? Entry
    {
        get => _entry;
        set => RaiseAndSetIfChanged(ref _entry, value);
    }

    public override async Task OnNavigating(WorkshopDetailParameters parameters, NavigationArguments args, CancellationToken cancellationToken)
    {
        await GetEntry(parameters.EntryId, cancellationToken);
    }

    private async Task GetEntry(Guid entryId, CancellationToken cancellationToken)
    {
        IOperationResult<IGetEntryByIdResult> result = await _client.GetEntryById.ExecuteAsync(entryId, cancellationToken);
        if (result.IsErrorResult())
            return;

        Entry = result.Data?.Entry;
    }
}