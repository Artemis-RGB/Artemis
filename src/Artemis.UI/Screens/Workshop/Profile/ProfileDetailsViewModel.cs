using System;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Screens.Workshop.Parameters;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Services;
using Avalonia.Media.Imaging;
using StrawberryShake;

namespace Artemis.UI.Screens.Workshop.Profile;

public class ProfileDetailsViewModel : RoutableScreen<ActivatableViewModelBase, WorkshopDetailParameters>, IWorkshopViewModel
{
    private readonly IWorkshopClient _client;
    private readonly IWorkshopService _workshopService;
    private IGetEntryById_Entry? _entry;
    private Bitmap? _entryIcon;

    public ProfileDetailsViewModel(IWorkshopClient client, IWorkshopService workshopService)
    {
        _client = client;
        _workshopService = workshopService;
    }

    public EntryType? EntryType => null;

    public IGetEntryById_Entry? Entry
    {
        get => _entry;
        set => RaiseAndSetIfChanged(ref _entry, value);
    }

    public Bitmap? EntryIcon
    {
        get => _entryIcon;
        set => RaiseAndSetIfChanged(ref _entryIcon, value);
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

        Bitmap? oldEntryIcon = EntryIcon;
        Entry = result.Data?.Entry;
        EntryIcon = await _workshopService.GetEntryIcon(entryId, cancellationToken);

        oldEntryIcon?.Dispose();
    }
}