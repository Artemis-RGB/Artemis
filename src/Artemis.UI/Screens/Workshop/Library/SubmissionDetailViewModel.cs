using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Screens.Workshop.Entries;
using Artemis.UI.Screens.Workshop.Parameters;
using Artemis.UI.Screens.Workshop.SubmissionWizard;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Utilities;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Exceptions;
using Artemis.WebClient.Workshop.Handlers.UploadHandlers;
using Artemis.WebClient.Workshop.Services;
using Avalonia.Media.Imaging;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using StrawberryShake;
using EntrySpecificationsViewModel = Artemis.UI.Screens.Workshop.Entries.Details.EntrySpecificationsViewModel;

namespace Artemis.UI.Screens.Workshop.Library;

public partial class SubmissionDetailViewModel : RoutableScreen<WorkshopDetailParameters>
{
    private readonly IWorkshopClient _client;
    private readonly Func<EntrySpecificationsViewModel> _getGetSpecificationsVm;
    private readonly IRouter _router;
    private readonly IWindowService _windowService;
    private readonly IWorkshopService _workshopService;
    [Notify] private IGetSubmittedEntryById_Entry? _entry;
    [Notify] private EntrySpecificationsViewModel? _entrySpecificationsViewModel;
    [Notify(Setter.Private)] private bool _hasChanges;

    public SubmissionDetailViewModel(IWorkshopClient client, IWindowService windowService, IWorkshopService workshopService, IRouter router, Func<EntrySpecificationsViewModel> getSpecificationsVm) {
        _client = client;
        _windowService = windowService;
        _workshopService = workshopService;
        _router = router;
        _getGetSpecificationsVm = getSpecificationsVm;

        CreateRelease = ReactiveCommand.CreateFromTask(ExecuteCreateRelease);
        DeleteSubmission = ReactiveCommand.CreateFromTask(ExecuteDeleteSubmission);
        ViewWorkshopPage = ReactiveCommand.CreateFromTask(ExecuteViewWorkshopPage);
        DiscardChanges = ReactiveCommand.CreateFromTask(ExecuteDiscardChanges, this.WhenAnyValue(vm => vm.HasChanges));
        SaveChanges = ReactiveCommand.CreateFromTask(ExecuteSaveChanges, this.WhenAnyValue(vm => vm.HasChanges));
    }

    public ReactiveCommand<Unit, Unit> CreateRelease { get; }
    public ReactiveCommand<Unit, Unit> DeleteSubmission { get; }
    public ReactiveCommand<Unit, Unit> ViewWorkshopPage { get; }
    public ReactiveCommand<Unit, Unit> SaveChanges { get; }
    public ReactiveCommand<Unit, Unit> DiscardChanges { get; }
    
    public override async Task OnNavigating(WorkshopDetailParameters parameters, NavigationArguments args, CancellationToken cancellationToken)
    {
        IOperationResult<IGetSubmittedEntryByIdResult> result = await _client.GetSubmittedEntryById.ExecuteAsync(parameters.EntryId, cancellationToken);
        if (result.IsErrorResult())
            return;

        Entry = result.Data?.Entry;
        await ApplyFromEntry(cancellationToken);
    }

    public override async Task OnClosing(NavigationArguments args)
    {
        if (!HasChanges)
            return;

        bool confirmed = await _windowService.ShowConfirmContentDialog("You have unsaved changes", "Do you want to discard your unsaved changes?");
        if (!confirmed)
            args.Cancel();
    }

    private async Task ApplyFromEntry(CancellationToken cancellationToken)
    {
        if (Entry == null)
            return;

        if (EntrySpecificationsViewModel != null)
        {
            EntrySpecificationsViewModel.PropertyChanged -= EntrySpecificationsViewModelOnPropertyChanged;
            ((INotifyCollectionChanged) EntrySpecificationsViewModel.SelectedCategories).CollectionChanged -= SelectedCategoriesOnCollectionChanged;
            EntrySpecificationsViewModel.Tags.CollectionChanged -= TagsOnCollectionChanged;
        }

        EntrySpecificationsViewModel viewModel = _getGetSpecificationsVm();

        viewModel.IconBitmap = await GetEntryIcon(cancellationToken);
        viewModel.Name = Entry.Name;
        viewModel.Summary = Entry.Summary;
        viewModel.Description = Entry.Description;
        viewModel.PreselectedCategories = Entry.Categories.Select(c => c.Id).ToList();

        viewModel.Tags.Clear();
        foreach (string tag in Entry.Tags.Select(c => c.Name))
            viewModel.Tags.Add(tag);

        EntrySpecificationsViewModel = viewModel;
        EntrySpecificationsViewModel.PropertyChanged += EntrySpecificationsViewModelOnPropertyChanged;
        ((INotifyCollectionChanged) EntrySpecificationsViewModel.SelectedCategories).CollectionChanged += SelectedCategoriesOnCollectionChanged;
        EntrySpecificationsViewModel.Tags.CollectionChanged += TagsOnCollectionChanged;
    }

    private async Task<Bitmap?> GetEntryIcon(CancellationToken cancellationToken)
    {
        if (Entry == null)
            return null;

        Stream? stream = await _workshopService.GetEntryIcon(Entry.Id, cancellationToken);
        return stream != null ? new Bitmap(stream) : null;
    }

    private void UpdateHasChanges()
    {
        if (EntrySpecificationsViewModel == null || Entry == null)
            return;

        List<long> categories = EntrySpecificationsViewModel.Categories.Where(c => c.IsSelected).Select(c => c.Id).OrderBy(c => c).ToList();
        List<string> tags = EntrySpecificationsViewModel.Tags.OrderBy(t => t).ToList();

        HasChanges = EntrySpecificationsViewModel.Name != Entry.Name ||
                     EntrySpecificationsViewModel.Description != Entry.Description ||
                     EntrySpecificationsViewModel.Summary != Entry.Summary ||
                     EntrySpecificationsViewModel.IconChanged ||
                     !tags.SequenceEqual(Entry.Tags.Select(t => t.Name).OrderBy(t => t)) ||
                     !categories.SequenceEqual(Entry.Categories.Select(c => c.Id).OrderBy(c => c));
    }

    private async Task ExecuteDiscardChanges()
    {
        await ApplyFromEntry(CancellationToken.None);
    }

    private async Task ExecuteSaveChanges(CancellationToken cancellationToken)
    {
        if (Entry == null || EntrySpecificationsViewModel == null || !EntrySpecificationsViewModel.ValidationContext.GetIsValid())
            return;
        
        UpdateEntryInput input = new()
        {
            Id = Entry.Id,
            Name = EntrySpecificationsViewModel.Name,
            Summary = EntrySpecificationsViewModel.Summary,
            Description = EntrySpecificationsViewModel.Description,
            Categories = EntrySpecificationsViewModel.SelectedCategories,
            Tags = EntrySpecificationsViewModel.Tags
        };

        IOperationResult<IUpdateEntryResult> result = await _client.UpdateEntry.ExecuteAsync(input, cancellationToken);
        result.EnsureNoErrors();

        if (EntrySpecificationsViewModel.IconChanged && EntrySpecificationsViewModel.IconBitmap != null)
        {
            using MemoryStream stream = new();
            EntrySpecificationsViewModel.IconBitmap.Save(stream);
            ImageUploadResult imageResult = await _workshopService.SetEntryIcon(Entry.Id, stream, cancellationToken);
            if (!imageResult.IsSuccess)
                throw new ArtemisWorkshopException("Failed to upload image. " + imageResult.Message);
        }

        HasChanges = false;
    }

    private async Task ExecuteCreateRelease(CancellationToken cancellationToken)
    {
        if (Entry != null)
            await _windowService.ShowDialogAsync<ReleaseWizardViewModel>(Entry);
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

    private async Task ExecuteViewWorkshopPage()
    {
        if (Entry != null)
            await _workshopService.NavigateToEntry(Entry.Id, Entry.EntryType);
    }
    
    private void TagsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateHasChanges();
    }

    private void SelectedCategoriesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateHasChanges();
    }

    private void EntrySpecificationsViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        UpdateHasChanges();
    }
}