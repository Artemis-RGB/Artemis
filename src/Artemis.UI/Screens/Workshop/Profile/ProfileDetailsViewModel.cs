using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Screens.Workshop.Parameters;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using Artemis.WebClient.Workshop;
using ReactiveUI;
using StrawberryShake;

namespace Artemis.UI.Screens.Workshop.Profile;

public class ProfileDetailsViewModel : RoutableScreen<ActivatableViewModelBase, WorkshopDetailParameters>, IWorkshopViewModel
{
    private readonly IWorkshopClient _client;
    private readonly ObservableAsPropertyHelper<DateTimeOffset?> _updatedAt;
    private IGetEntryById_Entry? _entry;

    public ProfileDetailsViewModel(IWorkshopClient client)
    {
        _client = client;
        _updatedAt = this.WhenAnyValue(vm => vm.Entry).Select(e => e?.LatestRelease?.CreatedAt ?? e?.CreatedAt).ToProperty(this, vm => vm.UpdatedAt);
    }

    public DateTimeOffset? UpdatedAt => _updatedAt.Value;

    public EntryType? EntryType => null;

    public IGetEntryById_Entry? Entry
    {
        get => _entry;
        private set => RaiseAndSetIfChanged(ref _entry, value);
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