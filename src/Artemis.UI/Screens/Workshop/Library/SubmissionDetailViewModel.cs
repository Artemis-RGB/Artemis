using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Screens.Workshop.Entries;
using Artemis.UI.Screens.Workshop.Parameters;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop;
using Avalonia.Media.Imaging;
using ReactiveUI;
using StrawberryShake;

namespace Artemis.UI.Screens.Workshop.Library;

public class SubmissionDetailViewModel : RoutableScreen<WorkshopDetailParameters>
{
    private readonly IWorkshopClient _client;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Func<EntrySpecificationsViewModel> _getEntrySpecificationsViewModel;
    private readonly IWindowService _windowService;
    private readonly IRouter _router;
    private IGetSubmittedEntryById_Entry? _entry;
    private EntrySpecificationsViewModel? _entrySpecificationsViewModel;

    public SubmissionDetailViewModel(IWorkshopClient client,
        IHttpClientFactory httpClientFactory, 
        Func<EntrySpecificationsViewModel> entrySpecificationsViewModel,
        IWindowService windowService,
        IRouter router)
    {
        _client = client;
        _httpClientFactory = httpClientFactory;
        _getEntrySpecificationsViewModel = entrySpecificationsViewModel;
        _windowService = windowService;
        _router = router;

        CreateRelease = ReactiveCommand.CreateFromTask(ExecuteCreateRelease);
        DeleteSubmission = ReactiveCommand.CreateFromTask(ExecuteDeleteSubmission);
    }

    public ReactiveCommand<Unit, Unit> CreateRelease { get; }
    public ReactiveCommand<Unit, Unit> DeleteSubmission { get; }

    public EntrySpecificationsViewModel? EntrySpecificationsViewModel
    {
        get => _entrySpecificationsViewModel;
        set => RaiseAndSetIfChanged(ref _entrySpecificationsViewModel, value);
    }

    public IGetSubmittedEntryById_Entry? Entry
    {
        get => _entry;
        set => RaiseAndSetIfChanged(ref _entry, value);
    }

    public override async Task OnNavigating(WorkshopDetailParameters parameters, NavigationArguments args, CancellationToken cancellationToken)
    {
        IOperationResult<IGetSubmittedEntryByIdResult> result = await _client.GetSubmittedEntryById.ExecuteAsync(parameters.EntryId, cancellationToken);
        if (result.IsErrorResult())
            return;

        Entry = result.Data?.Entry;
        await ApplyFromEntry(cancellationToken);
    }

    private async Task ApplyFromEntry(CancellationToken cancellationToken)
    {
        if (Entry == null)
            return;

        EntrySpecificationsViewModel viewModel = _getEntrySpecificationsViewModel();

        viewModel.IconBitmap = await GetEntryIcon(cancellationToken);
        viewModel.Name = Entry.Name;
        viewModel.Summary = Entry.Summary;
        viewModel.Description = Entry.Description;
        viewModel.PreselectedCategories = Entry.Categories.Select(c => c.Id).ToList();

        viewModel.Tags.Clear();
        foreach (string tag in Entry.Tags.Select(c => c.Name))
            viewModel.Tags.Add(tag);

        EntrySpecificationsViewModel = viewModel;
    }

    private async Task<Bitmap?> GetEntryIcon(CancellationToken cancellationToken)
    {
        if (Entry == null)
            return null;

        HttpClient client = _httpClientFactory.CreateClient(WorkshopConstants.WORKSHOP_CLIENT_NAME);
        try
        {
            HttpResponseMessage response = await client.GetAsync($"entries/{Entry.Id}/icon", cancellationToken);
            response.EnsureSuccessStatusCode();
            Stream data = await response.Content.ReadAsStreamAsync(cancellationToken);
            return new Bitmap(data);
        }
        catch (HttpRequestException)
        {
            // ignored
            return null;
        }
    }

    private Task ExecuteCreateRelease(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    private async Task ExecuteDeleteSubmission(CancellationToken cancellationToken)
    {
        if (Entry == null)
            return;
        
        bool confirmed = await _windowService.ShowConfirmContentDialog(
            "Delete submission?",
            "You cannot undo this by yourself.\r\n" +
            "Users that have already downloaded your submission will keep it.");
        if (!confirmed)
            return;

        IOperationResult<IRemoveEntryResult> result = await _client.RemoveEntry.ExecuteAsync(Entry.Id, cancellationToken);
        result.EnsureNoErrors();
        await _router.Navigate("workshop/library/submissions");
    }
}