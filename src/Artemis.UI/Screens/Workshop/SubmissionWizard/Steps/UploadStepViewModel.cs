using System;
using System.Linq;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Exceptions;
using Artemis.WebClient.Workshop.Handlers.UploadHandlers;
using Artemis.WebClient.Workshop.Services;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using Serilog;
using StrawberryShake;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard.Steps;

public partial class UploadStepViewModel : SubmissionViewModel
{
    private readonly ILogger _logger;
    private readonly EntryUploadHandlerFactory _entryUploadHandlerFactory;
    private readonly IRouter _router;
    private readonly IWindowService _windowService;
    private readonly IWorkshopClient _workshopClient;
    private readonly IWorkshopService _workshopService;
    private long? _entryId;
    [Notify] private bool _failed;
    [Notify] private bool _finished;
    [Notify] private bool _succeeded;
    [Notify] private string? _failureMessage;

    /// <inheritdoc />
    public UploadStepViewModel(ILogger logger,
        IWorkshopClient workshopClient,
        IWorkshopService workshopService,
        EntryUploadHandlerFactory entryUploadHandlerFactory,
        IWindowService windowService,
        IRouter router)
    {
        _logger = logger;
        _workshopClient = workshopClient;
        _workshopService = workshopService;
        _entryUploadHandlerFactory = entryUploadHandlerFactory;
        _windowService = windowService;
        _router = router;

        ShowGoBack = false;
        ContinueText = "Finish";
        Continue = ReactiveCommand.CreateFromTask(ExecuteContinue, this.WhenAnyValue(vm => vm.Finished));

        this.WhenActivated(d => Observable.FromAsync(ExecuteUpload).Subscribe().DisposeWith(d));
    }

    private async Task ExecuteUpload(CancellationToken cancellationToken)
    {
        try
        {
            // Use the existing entry or create a new one
            _entryId = State.EntryId ?? await CreateEntry(cancellationToken);
            if (_entryId == null)
            {
                Failed = true;
                return;
            }

            // Create a release for the new entry
            IEntryUploadHandler uploadHandler = _entryUploadHandlerFactory.CreateHandler(State.EntryType);
            EntryUploadResult uploadResult = await uploadHandler.CreateReleaseAsync(_entryId.Value, State.EntrySource!, State.Changelog, cancellationToken);
            if (!uploadResult.IsSuccess)
                throw new ArtemisWorkshopException(uploadResult.Message);

            Succeeded = true;
        }
        catch (Exception e)
        {
            _logger.Error(e, "Failed to upload submission for entry {EntryId}", _entryId);
            FailureMessage = e.Message;
            Failed = true;

            // If something went wrong halfway through, delete the entry if it was new
            if (State.EntryId == null && _entryId != null)
                await _workshopClient.RemoveEntry.ExecuteAsync(_entryId.Value, CancellationToken.None);
        }
        finally
        {
            Finished = true;
        }
    }

    private async Task<long> CreateEntry(CancellationToken cancellationToken)
    {
        // Let the UI settle before making the thread busy
        await Task.Delay(500, cancellationToken);

        // Create entry
        IOperationResult<IAddEntryResult> result = await _workshopClient.AddEntry.ExecuteAsync(new CreateEntryInput
        {
            EntryType = State.EntryType,
            Name = State.Name,
            Summary = State.Summary,
            Description = State.Description,
            Categories = State.Categories,
            Tags = State.Tags,
            DefaultEntryInfo = State.IsDefault
                ? new DefaultEntryInfoInput
                {
                    IsEssential = State.IsEssential,
                    IsDeviceProvider = State.IsDeviceProvider
                }
                : null
        }, cancellationToken);

        result.EnsureNoErrors();
        if (result.Data?.AddEntry == null)
            throw new ArtemisWorkshopException("AddEntry returned result");
        long entryId = result.Data.AddEntry.Id;

        // Upload images
        cancellationToken.ThrowIfCancellationRequested();
        foreach (ImageUploadRequest image in State.Images.ToList())
        {
            await TryImageUpload(async () => await _workshopService.UploadEntryImage(entryId, image, cancellationToken));
            cancellationToken.ThrowIfCancellationRequested();
        }

        if (State.Icon == null)
            return entryId;

        // Upload icon
        await TryImageUpload(async () => await _workshopService.SetEntryIcon(entryId, State.Icon, cancellationToken));
        return entryId;
    }

    private async Task TryImageUpload(Func<Task<ApiResult>> action)
    {
        try
        {
            ApiResult result = await action();
            if (!result.IsSuccess)
                throw new ArtemisWorkshopException(result.Message);
        }
        catch (Exception e)
        {
            // It's not critical if this fails
            await _windowService.ShowConfirmContentDialog("Failed to upload", "Your submission will continue, you can try upload a new image afterwards\r\n" + e.Message, "Continue", null);
        }
    }

    private async Task ExecuteContinue()
    {
        State.Close();

        if (Succeeded && _entryId != null)
            await _router.Navigate($"workshop/library/submissions/{_entryId.Value}");
    }
}