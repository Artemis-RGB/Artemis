using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Screens.Workshop.Parameters;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Builders;
using Artemis.WebClient.Workshop;
using Avalonia.Layout;
using AvaloniaEdit.Document;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using StrawberryShake;

namespace Artemis.UI.Screens.Workshop.Library;

public partial class SubmissionReleaseViewModel : RoutableScreen<ReleaseDetailParameters>
{
    private readonly IWorkshopClient _client;
    private readonly IRouter _router;
    private readonly IWindowService _windowService;
    private readonly INotificationService _notificationService;
    private readonly ObservableAsPropertyHelper<bool> _hasChanges;

    [Notify] private IGetReleaseById_Release? _release;
    [Notify] private string _changelog = string.Empty;
    [Notify] private TextDocument? _markdownDocument;

    public SubmissionReleaseViewModel(IWorkshopClient client, IRouter router, IWindowService windowService, INotificationService notificationService)
    {
        _client = client;
        _router = router;
        _windowService = windowService;
        _notificationService = notificationService;
        _hasChanges = this.WhenAnyValue(vm => vm.Changelog, vm => vm.Release, (current, release) => current != release?.Changelog).ToProperty(this, vm => vm.HasChanges);

        Discard = ReactiveCommand.Create(ExecuteDiscard, this.WhenAnyValue(vm => vm.HasChanges));
        Save = ReactiveCommand.CreateFromTask(ExecuteSave, this.WhenAnyValue(vm => vm.HasChanges));

        this.WhenActivated(d =>
        {
            Disposable.Create(() =>
            {
                if (MarkdownDocument != null)
                    MarkdownDocument.TextChanged -= MarkdownDocumentOnTextChanged;
            }).DisposeWith(d);
        });
    }

    public bool HasChanges => _hasChanges.Value;
    public ReactiveCommand<Unit, Unit> Discard { get; set; }
    public ReactiveCommand<Unit, Unit> Save { get; set; }

    public override async Task OnNavigating(ReleaseDetailParameters parameters, NavigationArguments args, CancellationToken cancellationToken)
    {
        IOperationResult<IGetReleaseByIdResult> result = await _client.GetReleaseById.ExecuteAsync(parameters.ReleaseId, cancellationToken);
        Release = result.Data?.Release;
        Changelog = Release?.Changelog ?? string.Empty;

        SetupMarkdownDocument();
    }

    public async Task DeleteRelease()
    {
        if (Release == null)
            return;

        bool confirmed = await _windowService.ShowConfirmContentDialog(
            "Delete release?",
            "This cannot be undone.\r\n" +
            "Users that have already downloaded this release will keep it.");
        if (!confirmed)
            return;
        
        await _client.RemoveRelease.ExecuteAsync(Release.Id);
        _notificationService.CreateNotification()
            .WithTitle("Deleted release.")
            .WithSeverity(NotificationSeverity.Success)
            .WithHorizontalPosition(HorizontalAlignment.Left)
            .Show();
        
        await Close();
    }

    public async Task Close()
    {
        await _router.GoUp();
    }
    
    private async Task ExecuteSave(CancellationToken cancellationToken)
    {
        if (Release == null)
            return;

        await _client.UpdateRelease.ExecuteAsync(new UpdateReleaseInput {Id = Release.Id, Changelog = Changelog}, cancellationToken);
        _notificationService.CreateNotification()
            .WithTitle("Saved changelog.")
            .WithSeverity(NotificationSeverity.Success)
            .WithHorizontalPosition(HorizontalAlignment.Left)
            .Show();
    }

    private void ExecuteDiscard()
    {
        Changelog = Release?.Changelog ?? string.Empty;
        SetupMarkdownDocument();
    }

    private void SetupMarkdownDocument()
    {
        if (MarkdownDocument != null)
            MarkdownDocument.TextChanged -= MarkdownDocumentOnTextChanged;

        MarkdownDocument = new TextDocument(new StringTextSource(Changelog));
        MarkdownDocument.TextChanged += MarkdownDocumentOnTextChanged;
    }

    private void MarkdownDocumentOnTextChanged(object? sender, EventArgs e)
    {
        Changelog = MarkdownDocument?.Text ?? string.Empty;
    }
}