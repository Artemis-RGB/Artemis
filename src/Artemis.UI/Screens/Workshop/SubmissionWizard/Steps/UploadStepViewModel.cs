using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.UploadHandlers;
using ReactiveUI;
using StrawberryShake;
using System.Reactive.Disposables;
using Artemis.Core;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard.Steps;

public class UploadStepViewModel : SubmissionViewModel
{
    private readonly IWorkshopClient _workshopClient;
    private readonly EntryUploadHandlerFactory _entryUploadHandlerFactory;
    private readonly IWindowService _windowService;
    private bool _finished;

    /// <inheritdoc />
    public UploadStepViewModel(IWorkshopClient workshopClient, EntryUploadHandlerFactory entryUploadHandlerFactory, IWindowService windowService)
    {
        _workshopClient = workshopClient;
        _entryUploadHandlerFactory = entryUploadHandlerFactory;
        _windowService = windowService;

        ShowGoBack = false;
        ContinueText = "Finish";
        Continue = ReactiveCommand.Create(ExecuteContinue, this.WhenAnyValue(vm => vm.Finished));

        this.WhenActivated(d => Observable.FromAsync(ExecuteUpload).Subscribe().DisposeWith(d));
    }

    /// <inheritdoc />
    public override ReactiveCommand<Unit, Unit> Continue { get; }

    /// <inheritdoc />
    public override ReactiveCommand<Unit, Unit> GoBack { get; } = null!;

    public bool Finished
    {
        get => _finished;
        set => RaiseAndSetIfChanged(ref _finished, value);
    }

    public async Task ExecuteUpload(CancellationToken cancellationToken)
    {
        IOperationResult<IAddEntryResult> result = await _workshopClient.AddEntry.ExecuteAsync(new CreateEntryInput
        {
            EntryType = State.EntryType,
            Name = State.Name,
            Summary = State.Summary,
            Description = State.Description,
            Categories = State.Categories,
            Tags = State.Tags
        }, cancellationToken);

        Guid? entryId = result.Data?.AddEntry?.Id;
        if (result.IsErrorResult() || entryId == null)
        {
            await _windowService.ShowConfirmContentDialog("Failed to create workshop entry", result.Errors.ToString() ?? "Not even an error message", "Close", null);
            return;
        }

        if (cancellationToken.IsCancellationRequested)
            return;

        // Create the workshop entry
        try
        {
            IEntryUploadHandler uploadHandler = _entryUploadHandlerFactory.CreateHandler(State.EntryType);
            EntryUploadResult uploadResult = await uploadHandler.CreateReleaseAsync(entryId.Value, State.EntrySource!, cancellationToken);
            if (!uploadResult.IsSuccess)
            {
                string? message = uploadResult.Message;
                if (message != null)
                    message += "\r\n\r\n";
                else
                    message = "";
                message += "Your submission has still been saved, you may try to upload a new release";
                await _windowService.ShowConfirmContentDialog("Failed to upload workshop entry", message, "Close", null);
                return;
            }

            Finished = true;
        }
        catch (Exception e)
        {
            // Something went wrong when creating a release :c
            // We'll keep the workshop entry so that the user can make changes and try again
        }
    }

    private void ExecuteContinue()
    {
        throw new NotImplementedException();
    }
}